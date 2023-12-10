
using System.Text.Json;
using System.Text.Json.Serialization;
using GameRec.Api.Models;

namespace GameRec.Api.Clients;
public class IgdbClient
{
    private Auth _auth = new();
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly HttpClient _httpClient;

    public IgdbClient(IHttpClientFactory clientFactory, string twitchId, string twitchSecret)
    {
        _clientId = twitchId;
        _clientSecret = twitchSecret;
        _httpClientFactory = clientFactory;
        _httpClient = _httpClientFactory.CreateClient();
    }

    public bool HasValidAuth()
    {
        return _auth.IsValid;
    }

    public async Task RefreshAuth()
    {
        var endpointParams = $"client_id={_clientId}&client_secret={_clientSecret}&grant_type=client_credentials";
        var endpoint = $"https://id.twitch.tv/oauth2/token?{endpointParams}";
        //using var client = _httpClientFactory.CreateClient();
        var response = await _httpClient.PostAsync(endpoint, null);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (content != null)
            {
                _auth = JsonSerializer.Deserialize<Auth>(content);
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

        //using var client = _httpClientFactory.CreateClient();
        var response = await _httpClient.SendAsync(httpRequestMessage);
        var content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode && content != null)
        {
            return JsonSerializer.Deserialize<T>(content);
        }
        else
        {
            Console.WriteLine("Post request failed. IGDB could not be queried");
            return default;
        }
    }

    public async Task<Game[]> FindGamesFromIds(int[] gameIds)
    {
        var whereIds = string.Join(", ", gameIds);
        var query =
            @$"
            fields name, genres, themes, player_perspectives,  game_modes, age_ratings, first_release_date;
            where id = ({whereIds});
            limit 500;
            ";
        var games = await Query<Game[]>("games", query);

        return games ?? Array.Empty<Game>();
    }

    public async Task<Game[]> SearchForGame(string name)
    {
        const int categoryMainGame = 0;

        var query =
            @$"
            fields id, name, first_release_date;
            where version_parent = null & category = {categoryMainGame} & first_release_date != null & name ~""{name}""*;
            sort  rating desc;
            limit 100;
            ";

        var games = await Query<Game[]>("games", query);

        return games ?? Array.Empty<Game>();
    }

    private struct Auth
    {
        public readonly bool IsValid
        {
            get
            {
                var isNotExpired = (RefreshedAt - DateTime.Now).Seconds < ExpiresIn;
                var isAccessTokenValid = AccessToken.Length > 0;
                return isAccessTokenValid && isNotExpired;
            }
        }

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
