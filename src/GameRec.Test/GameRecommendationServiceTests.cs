using GameRec.Api.Clients;
using GameRec.Api.Models;
using GameRec.Api.Services;
using Moq;

namespace GameRec.Test;

[TestFixture]
public class GameRecommendationServiceTests
{
    private static readonly Game.Genre[] _genres = Enum.GetValues<Game.Genre>();
    private static readonly Game.Theme[] _themes = Enum.GetValues<Game.Theme>();
    private static readonly Game.PlayerPerspective[] _playerPerspectives = Enum.GetValues<Game.PlayerPerspective>();
    private static readonly Game.GameMode[] _gameModes = Enum.GetValues<Game.GameMode>();
    private static readonly Game.AgeRating[] _ageRatings = Enum.GetValues<Game.AgeRating>();

    private GameRecommendationService _gameRecService;
    private Mock<IHttpClientFactory> _httpClientFactoryMock;

    [SetUp]
    public async Task Setup()
    {
        var clientId = Environment.GetEnvironmentVariable("TWITCH_CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable("TWITCH_CLIENT_SECRET");
        var client = new HttpClient();

        Assert.That(clientId, Is.Not.Null, "TWITCH_CLIENT_ID envrionment variable is required.");
        Assert.That(clientSecret, Is.Not.Null, "TWITCH_CLIENT_SECRET environment variable is required.");

        _httpClientFactoryMock = new();
        _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(new HttpClient());

        var igdbClient = new IgdbClient(_httpClientFactoryMock.Object, clientId, clientSecret);
        await igdbClient.RefreshAuth();
        _gameRecService = new GameRecommendationService(igdbClient);
    }

    [Test]
    public void CreateFeatureMatrix_GameArray_ReturnExpectedMatrix()
    {

        var games = new Game[]{
            new Game{
                Name = "Test",
                Genres = new int[]{(int)Game.Genre.PointAndClick},
                Themes = new int[]{(int)Game.Theme.Action},
                PlayerPerspectives = new int[]{(int)Game.PlayerPerspective.FirstPerson},
                GameModes = new int[]{(int)Game.GameMode.SinglePlayer},
                AgeRatings = new int[]{(int)Game.AgeRating.Three},
            }
        };

        var featMat = GameRecommendationService.CreateFeatureMatrix(games);
        Assert.Multiple(() =>
        {
            Assert.That(featMat[0, GetGenreStartIndex()].Real, Is.EqualTo(1.0), "Expected 'point and click' genre column to contain value of 1");
            Assert.That(featMat[0, GetThemeStartIndex()].Real, Is.EqualTo(1.0), "Expected 'action' theme column to contain value of 1");
            Assert.That(featMat[0, GetPerspectiveStartIndex()].Real, Is.EqualTo(1.0), "Expected 'first person' perspective column to contain value of 1");
            Assert.That(featMat[0, GetGameModeStartIndex()].Real, Is.EqualTo(1.0), "Expected 'single player' game mode column to contain value of 1");
            Assert.That(featMat[0, GetAgeRatingStartIndex()].Real, Is.EqualTo(1.0), "Expected 'three' age rating column to contain value of 1");
        });
    }

    [Test]
    public void GenerateUserProfileVector_RatedGameArray_ReturnExpectedVector()
    {
        var games = new RatedGame[]{
            new RatedGame{
                Rating = 1.0,
                Game = new Game{
                    Name = "Test",
                    Genres = new int[]{(int)Game.Genre.PointAndClick},
                    Themes = new int[]{(int)Game.Theme.Action},
                    PlayerPerspectives = new int[]{(int)Game.PlayerPerspective.FirstPerson},
                    GameModes = new int[]{(int)Game.GameMode.SinglePlayer},
                    AgeRatings = new int[]{(int)Game.AgeRating.Three},
                }

            }
        };

        var weightPerFeature = 1 / 5.0;
        var userProfileVector = GameRecommendationService.GenerateUserProfileVector(games);
        Assert.Multiple(() =>
        {
            Assert.That(userProfileVector[GetGenreStartIndex()].Real, Is.EqualTo(weightPerFeature), $"Expected 'point and click' genre column to contain value of {weightPerFeature}");
            Assert.That(userProfileVector[GetThemeStartIndex()].Real, Is.EqualTo(weightPerFeature), $"Expected 'action' theme column to contain value of {weightPerFeature}");
            Assert.That(userProfileVector[GetPerspectiveStartIndex()].Real, Is.EqualTo(weightPerFeature), $"Expected 'first person' perspective column to contain value of {weightPerFeature}");
            Assert.That(userProfileVector[GetGameModeStartIndex()].Real, Is.EqualTo(weightPerFeature), $"Expected 'single player' game mode column to contain value of {weightPerFeature}");
            Assert.That(userProfileVector[GetAgeRatingStartIndex()].Real, Is.EqualTo(weightPerFeature), $"Expected 'three' age rating column to contain value of {weightPerFeature}");
        });
    }

    [Test]
    public async Task FindCandidateGames_InputGames_ReturnAnyCandidates()
    {
        var games = new Game[]{
            new() {
                Name = "Test",
                Genres = new int[]{(int)Game.Genre.PointAndClick},
                Themes = new int[]{(int)Game.Theme.Action},
                PlayerPerspectives = new int[]{(int)Game.PlayerPerspective.FirstPerson},
                GameModes = new int[]{(int)Game.GameMode.SinglePlayer},
                AgeRatings = new int[]{(int)Game.AgeRating.Three},
            }
        };
        var candidateGames = await _gameRecService.FindCandidateGames(games);

        Assert.That(candidateGames, Is.Not.Empty);
    }

    [Test]
    public async Task GetRecommendedGames_InputGames_ReturnAnyRecommendations()
    {
        const int bloodBorneId = 7334;
        var recommendedGames = await _gameRecService.GetRecommendedGames(new Dictionary<int, double> { { bloodBorneId, 1.0 } });
        Assert.That(recommendedGames, Is.Not.Empty);
    }

    private int GetGenreStartIndex() => 0;
    private int GetThemeStartIndex() => _genres.Length;
    private int GetPerspectiveStartIndex() => GetThemeStartIndex() + _themes.Length;
    private int GetGameModeStartIndex() => GetPerspectiveStartIndex() + _playerPerspectives.Length;
    private int GetAgeRatingStartIndex() => GetGameModeStartIndex() + _gameModes.Length;
}
