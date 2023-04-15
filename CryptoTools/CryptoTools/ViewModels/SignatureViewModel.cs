using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Windows.UI;
using CryptoLib.Models;
using CryptoTools.Models;
using Microsoft.UI;

namespace CryptoTools.ViewModels;

public class SignatureViewModel : ViewModelBase
{
    public delegate void DisplayMessageDelegate(string message, Color color);

    private readonly SHA256 _sha256 = SHA256.Create();

    public DisplayMessageDelegate DisplayMessage;

    public async Task SignFile(string fileName)
    {
        await using var fs = new FileStream(fileName, FileMode.Open);
        var hash = await _sha256.ComputeHashAsync(fs);
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("X-Access-Token", Model.AccessToken);
        var response = await client.PostAsJsonAsync($"{Model.ServerUrl}/sign",
            new SignatureRequest
            {
                Hash = Convert.ToBase64String(hash)
            });
        var signature = await response.Content.ReadAsStringAsync();
        var signatureBytes = Convert.FromBase64String(signature);
        await using var outFs = new FileStream(fileName + ".sign", FileMode.Create);
        await outFs.WriteAsync(signatureBytes);
    }

    public async Task VerifySignature(string fileName)
    {
        await using var fs = new FileStream(fileName, FileMode.Open);
        var hash = await _sha256.ComputeHashAsync(fs);
        var signature = await File.ReadAllBytesAsync(fileName + ".sign");
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("X-Access-Token", Model.AccessToken);
        var response = await client.PostAsJsonAsync($"{Model.ServerUrl}/verify",
            new VerifyRequest
            {
                Hash = Convert.ToBase64String(hash),
                Signature = Convert.ToBase64String(signature)
            });
        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync();
            DisplayMessage?.Invoke(message, Colors.Coral);
            return;
        }

        DisplayMessage?.Invoke("Signature is valid.", Colors.Green);
    }
}