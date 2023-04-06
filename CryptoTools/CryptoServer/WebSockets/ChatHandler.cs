using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CryptoLib.Models;

namespace CryptoServer.WebSockets;

public class ChatHandler
{
    private readonly Dictionary<string, byte[]> _clientKeys = new();
    private readonly RSA _clientRsa = RSA.Create();
    private readonly RSA _serverRsa = RSA.Create();
    private readonly WebSocketManager _webSocketManager = new();

    public bool UserIsOn(string userName)
    {
        return _clientKeys.ContainsKey(userName);
    }

    public byte[] GetPublicKey()
    {
        return _serverRsa.ExportRSAPublicKey();
    }

    public void AddUser(string userName, byte[] publicKey)
    {
        _clientKeys.Add(userName, publicKey);
    }

    public async Task Handle(WebSocket webSocket)
    {
        var isFirstMessage = true;
        var sender = _webSocketManager.AddWebSocket(webSocket);
        await SendWelcomeMessage(sender);
        try
        {
            var buffer = new byte[4096];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                // Deserialize the message
                var chatMessage = JsonSerializer.Deserialize<ChatMessage>(json);
                if (chatMessage == null) continue;
                if (isFirstMessage)
                {
                    isFirstMessage = false;
                    // Store the sender's username
                    sender.UserName = chatMessage.UserName;
                    await BroadcastServerMessage($"{sender.UserName} joined the chat", sender);
                    continue;
                }

                await BroadcastMessage(chatMessage);

                buffer = new byte[4096];
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WebSocket connection closed: {ex.Message}");
        }
        finally
        {
            _webSocketManager.RemoveWebSocket(sender);
            await BroadcastServerMessage($"{sender.UserName} left the chat", sender);
            _clientKeys.Remove(sender.UserName);
        }
    }

    private async Task BroadcastServerMessage(string message, WebSocketConnection sender)
    {
        foreach (var connection in _webSocketManager.WebSockets)
            if (connection != sender)
                await SendUnencryptedMessage(message, connection);
    }

    private static async Task SendUnencryptedMessage(string message, WebSocketConnection connection)
    {
        var chatMessage = new ChatMessage
        {
            UserName = "Server",
            Message = Encoding.UTF8.GetBytes(message)
        };
        var json = JsonSerializer.Serialize(chatMessage);
        var buffer = Encoding.UTF8.GetBytes(json);
        await connection.WebSocket.SendAsync(
            new ArraySegment<byte>(buffer),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
        );
    }

    private async Task SendMessage(ChatMessage message, WebSocketConnection connection)
    {
        // Decrypt the message with the server's private key
        var decryptedSymmetricKey = _serverRsa.Decrypt(message.SymmetricKey, RSAEncryptionPadding.OaepSHA256);
        var decryptedHmacKey = _serverRsa.Decrypt(message.HmacKey, RSAEncryptionPadding.OaepSHA256);
        // Verify the integrity of the message
        using var hmac = new HMACSHA256(decryptedHmacKey);
        var hmacBytes = hmac.ComputeHash(message.Message);
        if (!hmacBytes.SequenceEqual(message.Hmac))
            throw new Exception("Message integrity check failed");
        // Encrypt the message with the client's public key
        var userName = connection.UserName;
        var userPublicKey = _clientKeys[userName];
        _clientRsa.ImportRSAPublicKey(userPublicKey, out _);
        var encryptedSymmetricKey = _clientRsa.Encrypt(decryptedSymmetricKey, RSAEncryptionPadding.OaepSHA256);
        var encryptedHmacKey = _clientRsa.Encrypt(decryptedHmacKey, RSAEncryptionPadding.OaepSHA256);
        // Send the message
        var messageToSend = new ChatMessage
        {
            UserName = message.UserName,
            Message = message.Message,
            Hmac = message.Hmac,
            SymmetricKey = encryptedSymmetricKey,
            HmacKey = encryptedHmacKey
        };
        var json = JsonSerializer.Serialize(messageToSend);
        var buffer = Encoding.UTF8.GetBytes(json);
        await connection.WebSocket.SendAsync(
            new ArraySegment<byte>(buffer),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
        );
    }

    private static async Task SendWelcomeMessage(WebSocketConnection webSocketConnection)
    {
        const string welcomeMessage = "Welcome to the chat room!";
        await SendUnencryptedMessage(welcomeMessage, webSocketConnection);
    }

    private async Task BroadcastMessage(ChatMessage message)
    {
        foreach (var connection in _webSocketManager.WebSockets)
            await SendMessage(message, connection);
    }
}