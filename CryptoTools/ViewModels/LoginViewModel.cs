using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CryptoLib;

namespace CryptoTools.ViewModels;

public class LoginViewModel : ViewModelBase
{
    public Action<string>? OnError;

    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public async Task<bool> Login()
    {
        using var client = Model.GetHttpClient();
        var response = await client.PostAsJsonAsync($"{Model.ServerUrl}/login", new LoginRequest(UserName, Password));
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

        if (!response.IsSuccessStatusCode || loginResponse is null)
        {
            OnError?.Invoke($"Login failed. Server responded with: {response.StatusCode}");
            return false;
        }

        Model.UserName = UserName;
        Model.AccessToken = loginResponse.AccessToken;
        await Model.OpenConnection();
        await Model.GetAlgorithms();
        return true;
    }
}