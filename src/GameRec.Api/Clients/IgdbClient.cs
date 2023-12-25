
using System.Text.Json;
using System.Text.Json.Serialization;
using GameRec.Api.Models;

namespace GameRec.Api.Clients;

/// <summary>
/// Represents a client for interacting with the IGDB (Internet Game Database) API.
/// </summary>
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
    /// <summary>
    /// Checks if the current auth token is valid.
    /// An auth token is considered valid if it has not exceeded its expiration time.
    /// </summary>
    /// <returns>True if the authentication is valid; otherwise, false.</returns>
    public bool HasValidAuth()
    {
        return _auth.IsValid;
    }

    /// <summary>
    /// Refreshes the auth token.
    /// This method must be called before using any client functionality to generate the initial authentication token.
    /// </summary>
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

    /// <summary>
    /// Queries the IGDB API with the specified endpoint and request body.
    /// </summary>
    /// <typeparam name="T">The type of the expected response.</typeparam>
    /// <param name="endpoint">The API endpoint.</param>
    /// <param name="body">The request body.</param>
    /// <returns>The deserialized response of type T.</returns>
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

    /// <summary>
    /// Finds games based on an array of IGDB game IDs.
    /// </summary>
    /// <param name="gameIds">The array of game IDs to search for.</param>
    /// <returns>An array of Game objects matching the specified IDs.</returns>
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

    /// <summary>
    /// Searches for games based on the provided name.
    /// The specified name is used as a filter to narrow down results to those closely approximating the given name.
    /// </summary>
    /// <param name="name">The name of the game to search for.</param>
    /// <returns>An array of Game objects approximating the specified name.</returns>
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

    /// <summary>
    /// Represents the auth token used by the IGDB client.
    /// </summary>
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
