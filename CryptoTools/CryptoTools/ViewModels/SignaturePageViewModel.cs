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
    private readonly SHA256 _sha256 = SHA256.Create();

    public DisplayMessageDelegate? DisplayMessage;

    private static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Next(s.Length)]).ToArray());
    }

    public void SignFiles(IEnumerable<string> dialogFileNames)
    {
        foreach (var fileName in dialogFileNames) SignFile(fileName);
        DisplayMessage?.Invoke("Files signed.", Colors.Green);
    }

    private async void SignFile(string fileName)
    {
        var randomString = RandomString(256);
        var randomBytes = Encoding.ASCII.GetBytes(randomString);
        var fileBytes = await File.ReadAllBytesAsync(fileName);
        var hash = _sha256.ComputeHash(randomBytes);
        using var client = new HttpClient();
        var response = await client.PostAsJsonAsync("https://cryptotools.azurewebsites.net/sign",
            new SignatureRequest
            {
                Data = Convert.ToBase64String(hash)
            });
        var signature = await response.Content.ReadAsStringAsync();
        var signatureBytes = Convert.FromBase64String(signature);
        var signatureFile = fileName + ".sig";
        await using var fs = new FileStream(signatureFile, FileMode.Create);
        await using var bw = new BinaryWriter(fs);
        bw.Write(signatureBytes);
        bw.Write(fileBytes);
    }

    public async void VerifySignature(string file)
    {
        await using var fs = new FileStream(file, FileMode.Open);
        using var br = new BinaryReader(fs);
        var signature = br.ReadBytes(1024);
        using var client = new HttpClient();
        var response = await client.PostAsJsonAsync("https://cryptotools.azurewebsites.net/verify",
            new SignatureRequest
            {
                Data = Convert.ToBase64String(signature)
            }
        );
        // Verify the signature
        var isSignatureValid = Convert.ToBoolean(await response.Content.ReadAsStringAsync());
        if (isSignatureValid)
            DisplayMessage?.Invoke("Signature is valid.", Colors.Green);
        else
            DisplayMessage?.Invoke("Signature is invalid.", Colors.Coral);
    }
}