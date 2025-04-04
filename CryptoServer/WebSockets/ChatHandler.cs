using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CryptoLib.Exceptions;
using CryptoLib.Models;
using CryptoServer.Utils;

namespace CryptoServer.WebSockets;

public class ChatHandler
{
    private readonly Dictionary<string, byte[]> _clientKeys = [];
    private readonly RSA _clientRsa = RSA.Create();
    private readonly RSA _serverRsa = RSA.Create();
    private readonly WebSocketManager _webSocketManager = new();

    public bool UserIsOnline(string userName) => _clientKeys.ContainsKey(userName);

    public byte[] GetPublicKey() => _serverRsa.ExportRSAPublicKey();

    public void AddUser(string userName, byte[] publicKey) => _clientKeys.Add(userName, publicKey);

    public async Task Handle(WebSocket webSocket, string username, string jwt, CancellationToken token)
    {
        var sender = _webSocketManager.AddWebSocket(webSocket, username, jwt);

        const string welcomeMessage = "Welcome to the chat room!";
        await SendUnencryptedMessage(sender, welcomeMessage, token);
        await BroadcastServerMessage(sender, $"{username} joined the chat", token);

        try
        {
            var buffer = new byte[4096];
            WebSocketReceiveResult result;

            do
            {
                Array.Clear(buffer, 0, buffer.Length);
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), token);

                if (!TokenUtils.ValidateAccessToken(sender.Jwt, sender.UserName))
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Unauthorized", token);
                    throw new ChatException("Access token is invalid");
                }

                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var message = JsonSerializer.Deserialize<ChatMessage>(json);
                if (message is null)
                    continue;

                var newMessage = message with
                {
                    SymmetricKey = _serverRsa.Decrypt(message.SymmetricKey, RSAEncryptionPadding.OaepSHA256),
                    HmacKey = _serverRsa.Decrypt(message.HmacKey, RSAEncryptionPadding.OaepSHA256)
                };
                await BroadcastMessage(newMessage, token);
            } while (!result.CloseStatus.HasValue);

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WebSocket connection closed: {ex.Message}");
        }
        finally
        {
            await BroadcastServerMessage(sender, $"{sender.UserName} left the chat", token);
            _webSocketManager.RemoveWebSocket(sender);
            _clientKeys.Remove(sender.UserName);
        }
    }

    private async Task BroadcastServerMessage(WebSocketConnection sender, string message, CancellationToken token)
    {
        foreach (var connection in _webSocketManager.WebSockets.Where(x => x != sender))
        {
            await SendUnencryptedMessage(connection, message, token);
        }
    }

    private static async Task SendUnencryptedMessage(WebSocketConnection connection, string message,
        CancellationToken token)
    {
        var chatMessage = new ChatMessage("Server", Encoding.UTF8.GetBytes(message));
        var json = JsonSerializer.Serialize(chatMessage);
        var buffer = Encoding.UTF8.GetBytes(json);
        await connection.WebSocket.SendAsync(
            new ArraySegment<byte>(buffer),
            WebSocketMessageType.Text,
            true,
            token
        );
    }

    private async Task SendMessage(WebSocketConnection connection, ChatMessage message, CancellationToken token)
    {
        var userPublicKey = _clientKeys[connection.UserName];
        _clientRsa.ImportRSAPublicKey(userPublicKey, out _);
        var encryptedSymmetricKey = _clientRsa.Encrypt(message.SymmetricKey, RSAEncryptionPadding.OaepSHA256);
        var encryptedHmacKey = _clientRsa.Encrypt(message.HmacKey, RSAEncryptionPadding.OaepSHA256);

        var messageToSend = new ChatMessage(message.UserName, message.Message, message.Hmac, encryptedSymmetricKey,
            encryptedHmacKey);
        var json = JsonSerializer.Serialize(messageToSend);
        var buffer = Encoding.UTF8.GetBytes(json);
        await connection.WebSocket.SendAsync(
            new ArraySegment<byte>(buffer),
            WebSocketMessageType.Text,
            true,
            token
        );
    }

    private async Task BroadcastMessage(ChatMessage message, CancellationToken token)
    {
        foreach (var connection in _webSocketManager.WebSockets)
        {
            await SendMessage(connection, message, token);
        }
    }
}