using Bitrix24API.Models;
using System.Text;
using System.Text.Json;

namespace Bitrix24API.Utils
{
    public class BitrixApiService
    {
        private BitrixAuthPayload _authStore;
        public BitrixApiService(BitrixAuthPayload authStore)
        {
            _authStore = authStore;
        }
        public async Task<string> CallApiAsync(string clientId, string clientSecret, string method, Dictionary<string, object> payload = null)
        {
            // Process when token is expired
            if (DateTimeOffset.FromUnixTimeSeconds(_authStore.Expires) <= DateTime.UtcNow)
            {
                _authStore = await RefreshToken(_authStore.RefreshToken, clientId, clientSecret);
            }

            var httpClient = new HttpClient(); 
            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{_authStore.ClientEndpoint}/{method}?auth={_authStore.AccessToken}", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Bitrix API error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        private async Task<BitrixAuthPayload> RefreshToken(string refreshToken, string clientId, string clientSecret)
        {
            var httpClient = new HttpClient();
            var url = $"https://oauth.bitrix.info/oauth/token/?grant_type=refresh_token&client_id={clientId}&client_secret={clientSecret}&refresh_token={refreshToken}";

            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new Exception($"Không thể refresh token do {message}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<JsonElement>(json);

            var newToken = new BitrixAuthPayload
            {
                AccessToken = obj.GetProperty("access_token").GetString(),
                RefreshToken = obj.GetProperty("refresh_token").GetString(),
                Expires = obj.GetProperty("expires").GetInt64(),
                ExpiresIn = obj.GetProperty("expires_in").GetInt64(),
                ClientEndpoint = obj.GetProperty("client_endpoint").GetString()
            };

            ManageItem.SaveToFile(newToken);


            return newToken;
        }
    }
}
