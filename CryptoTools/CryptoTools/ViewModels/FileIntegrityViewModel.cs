using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Media;
using CryptoLib.Models;
using CryptoLib.Services;

namespace CryptoTools.ViewModels;

public class FileIntegrityViewModel : ViewModelBase
{
    public delegate void DisplayMessageDelegate(string message, Color color);

    public DisplayMessageDelegate? DisplayMessage;

    public FileIntegrityViewModel()
    {
        SelectedAlgorithm = Model.HashingAlgorithms.First();
    }

    public HashingAlgorithm SelectedAlgorithm { get; set; }

    private async Task<bool> RegisterFile(string file)
    {
        if (!File.Exists(file)) return false;
        var fileName = Path.GetFileName(file);
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("X-Access-Token", Model.AccessToken);
        var hash = HashingService.GetFileHash(file, SelectedAlgorithm.Name);
        var hashEntry = new HashEntry
        {
            FileName = fileName,
            Hash = hash,
            HashingAlgorithmId = SelectedAlgorithm.Id
        };
        var response = await client.PostAsJsonAsync($"{Model.ServerUrl}/hash", hashEntry);
        return response.IsSuccessStatusCode;
    }

    public async void RegisterFiles(IEnumerable<string> files)
    {
        var allFilesRegistered = true;
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var file in files) allFilesRegistered &= await RegisterFile(file);
        if (!allFilesRegistered)
            DisplayMessage?.Invoke("Some files could not be registered!", Colors.Coral);
        else
            DisplayMessage?.Invoke("File(s) registered successfully.", Colors.Green);
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
        var hash = HashingService.GetFileHash(file, algorithmName ?? "SHA256");
        if (hashEntry.Hash == hash)
            DisplayMessage?.Invoke("File is valid.", Colors.Green);
        else
            DisplayMessage?.Invoke("File is invalid!", Colors.Red);
    }
}