using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Jojk.Models
{

    /* 

        I winged this, not sure if this is the best way at all. You could probably make changes. I have an "auto updater" that I made for another unreleased project I need to attach this here.

    */

    public class ConnectionManager : IDisposable
    {
        private readonly string _connectionString;
        private readonly string _appName = "Jojk";
        private readonly string _appVersion = "1.0";
        private readonly string _deviceID;
        private readonly string _deviceName;

        private string? _accessToken;
        private readonly HttpClient _httpClient;
        private string? _userID;

        public string UserID => _userID ?? throw new InvalidOperationException("User not authenticated");
        public HttpClient HTTPClient => _httpClient;
        public string AccessToken => _accessToken ?? throw new InvalidOperationException("User not authenticated");

        public ConnectionManager(string connectionURL)
        {
            _connectionString = connectionURL;
            _httpClient = new HttpClient() { BaseAddress = new Uri(_connectionString) };

            _deviceID = Guid.NewGuid().ToString();
            _deviceName = Environment.MachineName;

            _httpClient.DefaultRequestHeaders.Add("Authorization",
                $"MediaBrowser Client=\"{_appName}\", Device=\"{_deviceName}\", DeviceId=\"{_deviceID}\", Version=\"{_appVersion}\""
            );
        }

        public async Task AsyncAuth(string username, string password)
        {
            var authBody = new { Username = username, Pw = password };
            var authResponse = await _httpClient.PostAsJsonAsync("/Users/AuthenticateByName", authBody);
            authResponse.EnsureSuccessStatusCode();

            var jsonContent = await authResponse.Content.ReadFromJsonAsync<JsonElement>();
            _userID = jsonContent.GetProperty("User").GetProperty("Id").GetString();
            _accessToken = jsonContent.GetProperty("AccessToken").GetString();
            _httpClient.DefaultRequestHeaders.Add("X-Emby-Token", _accessToken);
        }

        public string FileStreamPath(string itemID)
        {
            var baseAddress = _httpClient.BaseAddress!.ToString().TrimEnd('/');
            return $"{baseAddress}/Audio/{itemID}/stream?static=true&api_key={Uri.EscapeDataString(_accessToken)}";
        }

        public string AlbumArtPath(string itemID, int maxWidth = 300, int maxHeight = 300)
        {
            var baseAddress = _httpClient.BaseAddress!.ToString().TrimEnd('/');
            return $"{baseAddress}/Items/{itemID}/Images/Primary?maxWidth={maxWidth}&maxHeight={maxHeight}&quality=90";
        }

        public void Dispose() => _httpClient.Dispose();
    }
}