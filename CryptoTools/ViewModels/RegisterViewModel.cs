using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CryptoLib;

namespace CryptoTools.ViewModels;

public class RegisterViewModel : ViewModelBase
{
    public Action<string>? OnError;

    public Action? RegisterSuccess;

    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string ConfirmPassword { get; set; } = string.Empty;


    public async Task<bool> Register()
    {
        if (UserName.Length < 5)
        {
            OnError?.Invoke("Username must be at least 5 characters long.");
            return false;
        }

        if (Password.Length < 8)
        {
            OnError?.Invoke("Password must be at least 8 characters long.");
            return false;
        }

        if (Password == ConfirmPassword)
        {
            OnError?.Invoke("Passwords do not match.");
            return false;
        }

        using var client = new HttpClient();
        var response = await client.PostAsJsonAsync($"{Model.ServerUrl}/register",
            new RegisterRequest(UserName, Password));

        if (!response.IsSuccessStatusCode)
        {
            OnError?.Invoke($"Registration failed. Server responded with: {response.StatusCode}");
            return false;
        }

        RegisterSuccess?.Invoke();
        return true;
    }
}