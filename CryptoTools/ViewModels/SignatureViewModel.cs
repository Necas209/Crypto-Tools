using System;
using System.IO;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Windows.UI;
using CryptoLib;
using Microsoft.UI;

namespace CryptoTools.ViewModels;

public class SignatureViewModel : ViewModelBase
{
    public delegate void DisplayMessageDelegate(string message, Color color);

    private readonly SHA256 _sha256 = SHA256.Create();

    public DisplayMessageDelegate? DisplayMessage;

    public async Task SignFile(string fileName)
    {
        await using var fs = new FileStream(fileName, FileMode.Open);
        var hash = await _sha256.ComputeHashAsync(fs);
        using var client = Model.GetHttpClient();
        var response = await client.PostAsJsonAsync($"{Model.ServerUrl}/sign",
            new SignatureRequest(Convert.ToBase64String(hash)));
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
        using var client = Model.GetHttpClient();
        var response = await client.PostAsJsonAsync($"{Model.ServerUrl}/verify",
            new VerifyRequest(Convert.ToBase64String(hash), Convert.ToBase64String(signature)));
        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync();
            DisplayMessage?.Invoke(message, Colors.Coral);
            return;
        }

        DisplayMessage?.Invoke("Signature is valid.", Colors.Green);
    }
}