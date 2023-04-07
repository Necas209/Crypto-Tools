using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CryptoLib.Models;
using CryptoTools.Utils;

namespace CryptoTools;

public class Model
{
    public const string ServerUrl = "https://cryptotools.azurewebsites.net";
    private const string ChatUrl = "wss://cryptotools.azurewebsites.net/chat";
    private readonly RSA _clientRsa = RSA.Create();
    private readonly RSA _serverRsa = RSA.Create();
    private ClientWebSocket _socket = new();

    public string AccessToken { get; set; } = string.Empty;

    public List<EncryptionAlgorithm> EncryptionAlgorithms { get; private set; } = new();

    public List<HashingAlgorithm> HashingAlgorithms { get; private set; } = new();

    public bool IsConnected => _socket.State == WebSocketState.Open;

    public async Task<string?> ReceiveMessage()
    {
        var buffer = new byte[4096];
        var result = await _socket.ReceiveAsync(buffer, CancellationToken.None);
        if (result.CloseStatus.HasValue)
            return result.CloseStatus is not WebSocketCloseStatus.NormalClosure ? null : "Logged out";
        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
        // Deserialize the message
        var chatMessage = JsonSerializer.Deserialize<ChatMessage>(message);
        if (chatMessage == null) return null;
        // If the message is from the server, it is not encrypted
        if (chatMessage.UserName == "Server") return Encoding.UTF8.GetString(chatMessage.Message);
        // Decrypt the message with the symmetric key
        var decryptedKey = _clientRsa.Decrypt(chatMessage.SymmetricKey, RSAEncryptionPadding.OaepSHA256);
        var hmacKey = _clientRsa.Decrypt(chatMessage.HmacKey, RSAEncryptionPadding.OaepSHA256);
        // Decrypt the message with the symmetric key
        var decryptedMessage = AesUtils.Decrypt(chatMessage.Message, decryptedKey);
        // Verify the HMAC of the decrypted message
        var hmac = HmacUtils.ComputeHmac(decryptedMessage, hmacKey);
        if (!hmac.SequenceEqual(chatMessage.Hmac)) throw new Exception("HMAC verification failed");
        // Convert the decrypted message to a string and attach the sender's username
        var decryptedMessageString = Encoding.UTF8.GetString(decryptedMessage);
        return $"{chatMessage.UserName}: {decryptedMessageString}";
    }

    public async Task SendMessage(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        // Compute the HMAC of the message
        var hmacKey = HmacUtils.GenerateHmacKey();
        var hmac = HmacUtils.ComputeHmac(messageBytes, hmacKey);
        // Generate an AES symmetric key
        using var aes = Aes.Create();
        aes.GenerateKey();
        var symmetricKey = aes.Key;
        // Encrypt the message with the symmetric key
        var encryptedMessage = AesUtils.Encrypt(messageBytes, symmetricKey);
        // Encrypt the symmetric key and HMAC key with the server's public key
        var encryptedSymmetricKey = _serverRsa.Encrypt(symmetricKey, RSAEncryptionPadding.OaepSHA256);
        var encryptedHmacKey = _serverRsa.Encrypt(hmacKey, RSAEncryptionPadding.OaepSHA256);
        // Create an ChatMessage instance
        var chatMessage = new ChatMessage
        {
            Message = encryptedMessage,
            Hmac = hmac,
            SymmetricKey = encryptedSymmetricKey,
            HmacKey = encryptedHmacKey
        };
        // Serialize the ChatMessage instance and send it to the server
        var json = JsonSerializer.Serialize(chatMessage);
        var buffer = Encoding.UTF8.GetBytes(json);
        await _socket.SendAsync(
            new ArraySegment<byte>(buffer),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
        );
    }

    public async Task OpenConnection()
    {
        // Retrieve the server's public key
        using var client = new HttpClient();
        var request = new ExchangeRequest
        {
            PublicKey = _clientRsa.ExportRSAPublicKey()
        };
        // Add the token to the request header
        client.DefaultRequestHeaders.Add("X-Access-Token", AccessToken);
        // Send the request to the server
        var response = await client.PostAsJsonAsync($"{ServerUrl}/exchange", request);
        var chatResponse = await response.Content.ReadAsStringAsync();
        if (chatResponse == null) throw new InvalidOperationException("Unable to retrieve server key");
        // Convert the server's public key from Base64 to a byte array
        var serverKey = Convert.FromBase64String(chatResponse);
        // Import the server's public key
        _serverRsa.ImportRSAPublicKey(serverKey, out _);
        // Connect to the server using a secure WebSocket
        _socket.Options.SetRequestHeader("X-Access-Token", AccessToken);
        var serverUri = new Uri(ChatUrl);
        await _socket.ConnectAsync(serverUri, CancellationToken.None);
    }

    public async Task CloseConnection()
    {
        await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Logout",
            CancellationToken.None);
        _socket.Dispose();
        _socket = new ClientWebSocket();
    }

    public async Task GetAlgorithms()
    {
        using var client = new HttpClient();
        EncryptionAlgorithms =
            await client.GetFromJsonAsync<List<EncryptionAlgorithm>>($"{ServerUrl}/encrypt")
            ?? throw new InvalidOperationException("Unable to retrieve encryption algorithms");
        HashingAlgorithms =
            await client.GetFromJsonAsync<List<HashingAlgorithm>>($"{ServerUrl}/hash")
            ?? throw new InvalidOperationException("Unable to retrieve hashing algorithms");
    }
}