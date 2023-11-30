
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameRec.Api.Repositories
{
    public class IGDBClient
    {
        private Auth? _auth;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly HttpClient _httpClient = new();

        public IGDBClient(string twitchId, string twitchSecret)
        {
            _clientId = twitchId;
            _clientSecret = twitchSecret;
        }

        public async Task RefreshAuth()
        {
            var endpointParams = $"client_id={_clientId}&client_secret={_clientSecret}&grant_type=client_credentials";
            var endpoint = $"https://id.twitch.tv/oauth2/token?{endpointParams}";
            var response = await _httpClient.PostAsync(endpoint, null);

            try
            {
                var content = await response.Content.ReadAsStringAsync();

                if (content != null)
                {
                    _auth = JsonSerializer.Deserialize<Auth>(content);
                    Console.WriteLine(_auth?.AccessToken);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        private struct Auth
        {
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

            public bool IsValid()
            {
                var timeSinceRefresh = RefreshedAt - DateTime.Now;
                return timeSinceRefresh.Seconds < ExpiresIn;
            }

        }
    }
}