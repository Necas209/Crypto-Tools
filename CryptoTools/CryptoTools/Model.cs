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
using CryptoLib.Utils;

namespace CryptoTools;

public class Model
{
    private readonly RSA _clientRsa = RSA.Create();
    private readonly RSA _serverRsa = RSA.Create();
    private ClientWebSocket _socket = new();

    public string UserName { get; set; } = string.Empty;

    public List<EncryptionAlgorithm> EncryptionAlgorithms { get; private set; } = new();

    public List<HashingAlgorithm> HashingAlgorithms { get; private set; } = new();

    public bool IsConnected => _socket.State == WebSocketState.Open;

    public async Task<string> ReceiveMessage()
    {
        var buffer = new byte[4096];
        var result = await _socket.ReceiveAsync(buffer, CancellationToken.None);
        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
        // Deserialize the message
        ChatMessage? chatMessage;
        try
        {
            chatMessage = JsonSerializer.Deserialize<ChatMessage>(message);
            if (chatMessage == null) return string.Empty;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return string.Empty;
        }

        var encryptedMessage = chatMessage.Message;
        // If the message is from the server, it is not encrypted
        if (chatMessage.UserName == "Server") return Encoding.UTF8.GetString(encryptedMessage);
        // Decrypt the message with the symmetric key
        var decryptedKey = _clientRsa.Decrypt(chatMessage.SymmetricKey,
            RSAEncryptionPadding.OaepSHA256);
        var decryptedHmacKey = _clientRsa.Decrypt(chatMessage.HmacKey,
            RSAEncryptionPadding.OaepSHA256);
        // Verify the HMAC of the encrypted message
        var hmac = HmacUtils.ComputeHmac(encryptedMessage, decryptedHmacKey);
        if (!hmac.SequenceEqual(chatMessage.Hmac)) throw new Exception("HMAC verification failed");
        // Decrypt the message with the symmetric key
        var decryptedMessage = AesUtils.Decrypt(encryptedMessage, decryptedKey);
        // Convert the decrypted message to a string and attach the sender's username
        var decryptedMessageString = Encoding.UTF8.GetString(decryptedMessage);
        return $"{chatMessage.UserName}: {decryptedMessageString}";
    }

    public async Task SendMessage(string message)
    {
        // Generate an AES symmetric key
        using var aes = Aes.Create();
        aes.GenerateKey();
        var symmetricKey = aes.Key;
        // Encrypt the message with the symmetric key
        var encryptedMessage = AesUtils.Encrypt(Encoding.UTF8.GetBytes(message), symmetricKey);
        // Compute the HMAC of the encrypted message
        var hmacKey = HmacUtils.GenerateHmacKey();
        var hmac = HmacUtils.ComputeHmac(encryptedMessage, hmacKey);
        // Encrypt the symmetric key and HMAC key with the server's public key
        var encryptedSymmetricKey = _serverRsa.Encrypt(symmetricKey, RSAEncryptionPadding.OaepSHA256);
        var encryptedHmacKey = _serverRsa.Encrypt(hmacKey, RSAEncryptionPadding.OaepSHA256);
        // Create an ChatMessage instance
        var chatMessage = new ChatMessage
        {
            UserName = UserName,
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
        var request = new ChatRequest
        {
            UserName = UserName,
            PublicKey = _clientRsa.ExportRSAPublicKey()
        };
        // Send the request to the server
        var response = await client.PostAsJsonAsync("https://cryptotools.azurewebsites.net/keys", request);
        var chatResponse = await response.Content.ReadAsStringAsync();
        if (chatResponse == null) throw new InvalidOperationException("Unable to retrieve server key");
        // Convert the server's public key from Base64 to a byte array
        var serverKey = Convert.FromBase64String(chatResponse);
        // Import the server's public key
        _serverRsa.ImportRSAPublicKey(serverKey, out _);
        // Connect to the server using a secure WebSocket
        var serverUri = new Uri("wss://cryptotools.azurewebsites.net/chat");
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
            await client.GetFromJsonAsync<List<EncryptionAlgorithm>>("https://cryptotools.azurewebsites.net/encrypt")
            ?? throw new InvalidOperationException("Unable to retrieve encryption algorithms");
        HashingAlgorithms =
            await client.GetFromJsonAsync<List<HashingAlgorithm>>("https://cryptotools.azurewebsites.net/hash")
            ?? throw new InvalidOperationException("Unable to retrieve hashing algorithms");
    }
}