using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media;
using CryptoLib.Models;

namespace CryptoTools.ViewModels;

public class SignaturePageViewModel
{
    public delegate void DisplayMessageDelegate(string message, Color color);

    private static readonly Random Random = new();

    private readonly RSA _rsa = RSA.Create();
    private readonly SHA256 _sha256 = SHA256.Create();

    public event DisplayMessageDelegate? DisplayMessage;

    private static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Next(s.Length)]).ToArray());
    }

    public void SignFiles(IEnumerable<string> dialogFileNames)
    {
        foreach (var fileName in dialogFileNames) SignFile(fileName);
    }

    private async void SignFile(string fileName)
    {
        var randomString = RandomString(256);
        var randomBytes = Encoding.ASCII.GetBytes(randomString);
        var file = await File.ReadAllBytesAsync(fileName);
        var hash = _sha256.ComputeHash(randomBytes);
        using var client = new HttpClient();
        var response = await client.PostAsJsonAsync("https://cryptotools.azurewebsites.net/sign",
            new SignRequest
            {
                Data = Encoding.ASCII.GetString(hash)
            }
        );
        var signature = await response.Content.ReadAsByteArrayAsync();
        var signatureFileBytes = CombineTwoByteArrays(signature, file);
        var signatureFile = fileName + ".signature";
        await File.WriteAllBytesAsync(signatureFile, signatureFileBytes);
    }

    private static byte[] CombineTwoByteArrays(byte[] signature, byte[] file)
    {
        var signatureFileBytes = new byte[signature.Length + file.Length];
        Buffer.BlockCopy(signature, 0, signatureFileBytes, 0, signature.Length);
        Buffer.BlockCopy(file, 0, signatureFileBytes, signature.Length, file.Length);
        return signatureFileBytes;
    }

    public async void VerificationSign(string file)
    {
        await using var fs = new FileStream(file, FileMode.Open);
        using var br = new BinaryReader(fs);
        var dataLength = br.ReadInt32();
        var data = br.ReadBytes(dataLength);
        var signatureLength = br.ReadInt32();
        var signature = br.ReadBytes(signatureLength);
        using var client = new HttpClient();
        var response = await client.PostAsJsonAsync("https://cryptotools.azurewebsites.net/verify",
            new SignRequest
            {
                Data = Encoding.ASCII.GetString(data),
                Signature = Encoding.ASCII.GetString(signature)
            }
        );
        // Verify the signature
        var isSignatureValid = await response.Content.ReadFromJsonAsync<bool>();
        if (isSignatureValid)
            DisplayMessage?.Invoke("Signature is valid.", Colors.Green);
        else
            DisplayMessage?.Invoke("Signature is invalid.", Colors.Coral);
    }
}