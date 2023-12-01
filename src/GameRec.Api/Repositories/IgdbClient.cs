
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameRec.Api.Repositories
{
    public class IgdbClient
    {
        private Auth _auth = new();
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly IHttpClientFactory _httpClientFactory;

        public IgdbClient(IHttpClientFactory clientFactory, string twitchId, string twitchSecret)
        {
            _clientId = twitchId;
            _clientSecret = twitchSecret;
            _httpClientFactory = clientFactory;
        }

        public async Task RefreshAuth()
        {
            var endpointParams = $"client_id={_clientId}&client_secret={_clientSecret}&grant_type=client_credentials";
            var endpoint = $"https://id.twitch.tv/oauth2/token?{endpointParams}";
            using var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync(endpoint, null);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                if (content != null)
                {
                    _auth = JsonSerializer.Deserialize<Auth>(content);
                    Console.WriteLine(_auth.AccessToken);
                }
            }
            else
            {
                Console.WriteLine("Failed to refresh auth");
            }
        }

        public async Task<T?> Query<T>(string endpoint, string body)
        {
            if (!_auth.IsValid)
            {
                Console.WriteLine("Auth invalid");
                return default;
            }

            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"https://api.igdb.com/v4/{endpoint}"),
                Headers = {
                    { "Authorization", $"{_auth.TokenType} {_auth.AccessToken}" },
                    { "Client-ID", $"{_clientId}" }
                },
                Content = new StringContent(body)
            };

            using var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(httpRequestMessage);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode && content != null)
            {
                Console.WriteLine(content);
                return JsonSerializer.Deserialize<T>(content);
            }
            else
            {
                Console.WriteLine("Post request failed. IGDB could not be queried");
                return default;
            }
        }


        private struct Auth
        {
            public readonly bool IsValid { get => (RefreshedAt - DateTime.Now).Seconds < ExpiresIn; }
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; } = "";
            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; } = 0;
            [JsonPropertyName("token_type")]
            public string TokenType { get; set; } = "";
            [JsonIgnore]
            public DateTime RefreshedAt { get; set; }

            public Auth()
            {
                RefreshedAt = DateTime.Now;
            }

        }
    }
}