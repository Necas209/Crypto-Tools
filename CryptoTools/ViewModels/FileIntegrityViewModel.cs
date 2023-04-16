using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using CryptoLib.Models;
using CryptoTools.Models;
using CryptoTools.Utils;
using Microsoft.UI;

namespace CryptoTools.ViewModels;

public class FileIntegrityViewModel : ViewModelBase
{
    public delegate void DisplayMessageDelegate(string message, Color color);

    public DisplayMessageDelegate DisplayMessage;

    public FileIntegrityViewModel()
    {
        SelectedAlgorithm = Model.HashingAlgorithms.First();
    }

    public List<HashingAlgorithm> Algorithms => Model.HashingAlgorithms;

    public HashingAlgorithm SelectedAlgorithm { get; set; }

    private async Task<bool> RegisterFile(string file)
    {
        if (!File.Exists(file)) return false;
        var fileName = Path.GetFileName(file);
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("X-Access-Token", Model.AccessToken);
        var hash = HashingUtils.HashFile(file, SelectedAlgorithm.Name);
        var hashEntry = new HashEntry
        {
            FileName = fileName,
            Hash = HashingUtils.ToHexString(hash),
            HashingAlgorithmId = SelectedAlgorithm.Id
        };
        var response = await client.PostAsJsonAsync($"{Model.ServerUrl}/hash", hashEntry);
        return response.IsSuccessStatusCode;
    }

    public async void RegisterFiles(IEnumerable<StorageFile> files)
    {
        var registered = await Task.WhenAll(files.Select(f => RegisterFile(f.Path)));
        if (registered.All(r => r))
            DisplayMessage?.Invoke("File(s) registered successfully.", Colors.Green);
        else
            DisplayMessage?.Invoke("Some files could not be registered!", Colors.Coral);
    }

    public async void ValidateFile(string file)
    {
        var fileName = Path.GetFileName(file);
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("X-Access-Token", Model.AccessToken);
        var encodedFileName = WebUtility.UrlEncode(fileName);
        var hashEntry = await client.GetFromJsonAsync<HashEntry>(
            $"{Model.ServerUrl}/hash/{encodedFileName}");
        if (hashEntry is null)
        {
            DisplayMessage?.Invoke("File could not be found!", Colors.Coral);
            return;
        }

        var algorithmName = Model.HashingAlgorithms
            .SingleOrDefault(a => a.Id == hashEntry.HashingAlgorithmId)?.Name;
        var hash = HashingUtils.HashFile(file, algorithmName ?? "SHA256");
        var hexHash = HashingUtils.ToHexString(hash);
        if (hashEntry.Hash == hexHash)
            DisplayMessage?.Invoke("File is valid.", Colors.Green);
        else
            DisplayMessage?.Invoke("File is invalid!", Colors.Red);
    }
}