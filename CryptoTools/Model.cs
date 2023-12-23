using System;
using System.Collections.Generic;
using System.IO;
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
    private readonly Uri _chatUri = new("wss://cryptotools.azurewebsites.net/chat");
    private readonly RSA _clientRsa = RSA.Create();
    private readonly RSA _serverRsa = RSA.Create();

    public readonly string AppFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "CryptoTools"
    );

    private ClientWebSocket _socket = new();

    public Model()
    {
        Directory.CreateDirectory(AppFolder);
    }

    public string UserName { get; set; } = string.Empty;

    public string AccessToken { get; set; } = string.Empty;

    public List<EncryptionAlgorithm> EncryptionAlgorithms { get; private set; } = [];

    public List<HashingAlgorithm> HashingAlgorithms { get; private set; } = [];

    public bool IsConnected => _socket.State == WebSocketState.Open;

    public async Task<string?> ReceiveMessage()
    {
        var buffer = new byte[4096];
        var result = await _socket.ReceiveAsync(buffer, CancellationToken.None);
        if (result.CloseStatus.HasValue)
            return result.CloseStatus is not WebSocketCloseStatus.NormalClosure ? null : "Logged out";

        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
        var chatMessage = JsonSerializer.Deserialize<ChatMessage>(message);
        if (chatMessage == null)
            return null;

        if (chatMessage.UserName == "Server")
            return Encoding.UTF8.GetString(chatMessage.Message);

        var decryptedKey = _clientRsa.Decrypt(chatMessage.SymmetricKey, RSAEncryptionPadding.OaepSHA256);
        var hmacKey = _clientRsa.Decrypt(chatMessage.HmacKey, RSAEncryptionPadding.OaepSHA256);
        var decryptedMessage = AesUtils.Decrypt(chatMessage.Message, decryptedKey);

        var hmac = HMACSHA256.HashData(hmacKey, decryptedMessage);
        if (!hmac.SequenceEqual(chatMessage.Hmac))
            throw new Exception("HMAC verification failed");

        var decryptedMessageString = Encoding.UTF8.GetString(decryptedMessage);
        return $"{chatMessage.UserName}: {decryptedMessageString}";
    }

    public async Task SendMessage(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        var hmacKey = RandomNumberGenerator.GetBytes(32);
        var hmac = HMACSHA256.HashData(hmacKey, messageBytes);

        using var aes = Aes.Create();
        aes.GenerateKey();

        var encryptedMessage = AesUtils.Encrypt(messageBytes, aes.Key);
        var encryptedSymmetricKey = _serverRsa.Encrypt(aes.Key, RSAEncryptionPadding.OaepSHA256);
        var encryptedHmacKey = _serverRsa.Encrypt(hmacKey, RSAEncryptionPadding.OaepSHA256);
        var chatMessage = new ChatMessage
        {
            UserName = UserName,
            Message = encryptedMessage,
            Hmac = hmac,
            SymmetricKey = encryptedSymmetricKey,
            HmacKey = encryptedHmacKey
        };

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
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("X-Access-Token", AccessToken);

        var response = await client.PostAsJsonAsync($"{ServerUrl}/exchange", new ExchangeRequest
        {
            PublicKey = _clientRsa.ExportRSAPublicKey()
        });
        var content = await response.Content.ReadAsStringAsync();
        if (content is null) 
            throw new InvalidOperationException("Unable to retrieve server key");

        var serverKey = Convert.FromBase64String(content);
        _serverRsa.ImportRSAPublicKey(serverKey, out _);
        _socket.Options.SetRequestHeader("X-Access-Token", AccessToken);
        await _socket.ConnectAsync(_chatUri, CancellationToken.None);
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

    public async Task SaveToken()
    {
        await File.WriteAllTextAsync(Path.Combine(AppFolder, "token.txt"), AccessToken);
    }

    public async Task<bool> IsTokenValid()
    {
        var tokenPath = Path.Combine(AppFolder, "token.txt");
        if (!File.Exists(tokenPath))
            return false;

        AccessToken = await File.ReadAllTextAsync(tokenPath);
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("X-Access-Token", AccessToken);

        var response = await client.GetAsync($"{ServerUrl}/is-logged-in");
        return response.IsSuccessStatusCode;
    }
}