using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CryptoLib.Models;
using CryptoServer.Utils;

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

    public async Task Handle(WebSocket webSocket, string username, string token)
    {
        var sender = _webSocketManager.AddWebSocket(webSocket, username, token);
        // Send the welcome message
        const string welcomeMessage = "Welcome to the chat room!";
        await SendUnencryptedMessage(sender, welcomeMessage);
        // Broadcast the new user's arrival
        await BroadcastServerMessage(sender, $"{username} joined the chat");
        try
        {
            var buffer = new byte[4096];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                if (!TokenUtils.ValidateAccessToken(sender.Token, sender.UserName))
                {
                    // If the access token is invalid or does not belong to the current user, close the WebSocket connection
                    await webSocket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Unauthorized",
                        CancellationToken.None);
                    throw new Exception("Access token is invalid");
                }

                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                // Deserialize the message
                var chatMessage = JsonSerializer.Deserialize<ChatMessage>(json);
                if (chatMessage == null) continue;
                // The server should not trust the client's username but instead, retrieve it from the connection object
                chatMessage.UserName = sender.UserName;
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
            // Broadcast the user's departure
            await BroadcastServerMessage(sender, $"{sender.UserName} left the chat");
            // Remove the user from the list of connected users
            _webSocketManager.RemoveWebSocket(sender);
            // Remove the user's public key
            _clientKeys.Remove(sender.UserName);
        }
    }

    private async Task BroadcastServerMessage(WebSocketConnection sender, string message)
    {
        foreach (var connection in _webSocketManager.WebSockets.Where(x => x != sender))
            await SendUnencryptedMessage(connection, message);
    }

    private static async Task SendUnencryptedMessage(WebSocketConnection connection, string message)
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

    private async Task SendMessage(WebSocketConnection connection, ChatMessage message)
    {
        // Decrypt the message with the server's private key
        var symmetricKey = _serverRsa.Decrypt(message.SymmetricKey, RSAEncryptionPadding.OaepSHA256);
        var hmacKey = _serverRsa.Decrypt(message.HmacKey, RSAEncryptionPadding.OaepSHA256);
        // Encrypt the message with the client's public key
        var userPublicKey = _clientKeys[connection.UserName];
        _clientRsa.ImportRSAPublicKey(userPublicKey, out _);
        var encryptedSymmetricKey = _clientRsa.Encrypt(symmetricKey, RSAEncryptionPadding.OaepSHA256);
        var encryptedHmacKey = _clientRsa.Encrypt(hmacKey, RSAEncryptionPadding.OaepSHA256);
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

    private async Task BroadcastMessage(ChatMessage message)
    {
        foreach (var connection in _webSocketManager.WebSockets)
            await SendMessage(connection, message);
    }
}