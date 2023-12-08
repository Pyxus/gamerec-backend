using GameRec.Api.Clients;
using GameRec.Api.Models;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace GameRec.Api.Services;

public class GameRecommendationService
{
    private readonly IgdbClient _igdbClient;
    public GameRecommendationService(IgdbClient igdbClient)
    {
        _igdbClient = igdbClient;
    }
    public async Task<RatedGame[]> GetRecommendedGames(Dictionary<int, double> ratingById)
    {
        var gameIds = ratingById.Keys.ToArray();
        var inputGames = await _igdbClient.FindGamesFromIds(gameIds);
        var ratedGames = inputGames.Select(g => new RatedGame(g, ratingById[g.Id])).ToArray();
        var candidateGames = await FindCandidateGames(inputGames);
        var candidateMatrix = CreateFeatureMatrix(candidateGames);
        var userProfileVector = GenerateUserProfileVector(ratedGames);
        var recommendedGamesVector = candidateMatrix * userProfileVector;
        var recommendedGames = new RatedGame[recommendedGamesVector.Count];

        for (int i = 0; i < recommendedGamesVector.Count; i++)
        {
            recommendedGames[i] = new RatedGame(candidateGames[i], (double)recommendedGamesVector[i].Real);
        }

        var orderedRecommendedGames = recommendedGames.OrderByDescending(rg => rg.Rating).ToArray();

        return orderedRecommendedGames ?? Array.Empty<RatedGame>();
    }

    public async Task<Game[]> FindCandidateGames(Game[] inputGames)
    {
        var featureSet = CreateFeatureSet(inputGames);
        var query =
            @$"
            fields name, genres, themes, player_perspectives,  game_modes, age_ratings, first_release_date;
            where genres =
                ({string.Join(", ", featureSet.Genres)})
                & themes = ({string.Join(", ", featureSet.Themes)})
                & player_perspectives = ({string.Join(", ", featureSet.PlayerPerspectives)})
                & game_modes = ({string.Join(", ", featureSet.GameModes)})
                & id != ({string.Join(", ", inputGames.Select(game => game.Id))})
                & rating >= 6;
            limit 500;
            ";
        var games = await _igdbClient.Query<Game[]>("games", query);


        return games ?? Array.Empty<Game>();
    }

    public static SparseMatrix CreateFeatureMatrix(Game[] games)
    {
        var genres = Enum.GetValues<Game.Genre>();
        var themes = Enum.GetValues<Game.Theme>();
        var playerPerspectives = Enum.GetValues<Game.PlayerPerspective>();
        var gameModes = Enum.GetValues<Game.GameMode>();
        var ageRatings = Enum.GetValues<Game.AgeRating>();
        var totalFeatures = genres.Length + themes.Length + playerPerspectives.Length + gameModes.Length + ageRatings.Length;
        var featureMatrix = new SparseMatrix(games.Length, totalFeatures);

        for (int row = 0; row < featureMatrix.RowCount; row++)
        {
            var game = games[row];
            var column = 0;

            foreach (var genre in genres)
            {
                if (game.Genres.Contains((int)genre))
                    featureMatrix[row, column] = 1.0;
                column++;
            }

            foreach (var theme in themes)
            {
                if (game.Themes.Contains((int)theme))
                    featureMatrix[row, column] = 1.0;
                column++;
            }

            foreach (var perspective in playerPerspectives)
            {
                if (game.PlayerPerspectives.Contains((int)perspective))
                    featureMatrix[row, column] = 1.0;
                column++;
            }

            foreach (var gameMode in gameModes)
            {
                if (game.GameModes.Contains((int)gameMode))
                    featureMatrix[row, column] = 1.0;
                column++;
            }

            foreach (var ageRating in ageRatings)
            {
                if (game.AgeRatings.Contains((int)ageRating))
                    featureMatrix[row, column] = 1.0;
                column++;
            }
        }

        return featureMatrix;
    }

    public static DenseVector GenerateUserProfileVector(RatedGame[] ratedGames)
    {
        var games = new Game[ratedGames.Length];
        var userRatingVector = new DenseVector(ratedGames.Length);

        for (int i = 0; i < ratedGames.Length; i++)
        {
            games[i] = ratedGames[i].Game;
            userRatingVector[i] = ratedGames[i].Rating;
        }

        var featureMatrix = CreateFeatureMatrix(games);
        var weightedFeatureVector = featureMatrix.Transpose() * userRatingVector;
        var userProfileMatrix = weightedFeatureVector / weightedFeatureVector.Sum();

        return (DenseVector)userProfileMatrix;
    }

    private static FeatureSet CreateFeatureSet(Game[] games)
    {
        var set = new FeatureSet();

        foreach (var game in games)
        {
            set.Genres.UnionWith(game.Genres);
            set.Themes.UnionWith(game.Themes);
            set.PlayerPerspectives.UnionWith(game.PlayerPerspectives);
            set.GameModes.UnionWith(game.GameModes);
            set.AgeRatings.UnionWith(game.AgeRatings);
        }

        return set;
    }

    private class FeatureSet
    {
        public HashSet<int> Genres = new();
        public HashSet<int> Themes = new();
        public HashSet<int> PlayerPerspectives = new();
        public HashSet<int> GameModes = new();
        public HashSet<int> AgeRatings = new();
    }
}
