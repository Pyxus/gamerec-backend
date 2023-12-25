using GameRec.Api.Clients;
using GameRec.Api.Models;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace GameRec.Api.Services;

/// <summary>
/// Service for providing content-based game recommendations based on the weighted features of user's input games.
/// </summary>
public class GameRecommendationService
{
    private readonly IgdbClient _igdbClient;
    public GameRecommendationService(IgdbClient igdbClient)
    {
        _igdbClient = igdbClient;
    }

    /// <summary>
    /// Gets recommended games based on user ratings using a content-based recommendation algorithm.
    /// </summary>
    /// <param name="ratingById">A dictionary containing game IDs and their corresponding user rating.</param>
    /// <returns>An array of rated games recommended for the user.</returns>
    public async Task<RatedGame[]> GetRecommendedGames(Dictionary<int, double> ratingById)
    {

        // Setp 1: Retrieve IGDB data based on input game ids; then re-associate each game with their user rating.
        var gameIds = ratingById.Keys.ToArray();
        var inputGames = await _igdbClient.FindGamesFromIds(gameIds);
        var ratedGames = inputGames.Select(g => new RatedGame { Game = g, Rating = ratingById[g.Id] }).ToArray();

        // Step 2: Construct a candidate matrix.A candidate refers to any game which is being considered for recommendation.
        var candidateGames = await FindCandidateGames(inputGames);
        var candidateMatrix = CreateFeatureMatrix(candidateGames);

        // Step 3: Generate user profile based on input games.
        var userProfileVector = GenerateUserProfileVector(ratedGames);

        // Step 4: Multiply candidate matrix by user profile to weight each candidate game
        // This list, once ordered, is our list of recommended games.
        var recommendedGamesVector = candidateMatrix * userProfileVector;
        var recommendedGames = new RatedGame[recommendedGamesVector.Count];

        for (int i = 0; i < recommendedGamesVector.Count; i++)
        {
            recommendedGames[i] = new RatedGame { Game = candidateGames[i], Rating = recommendedGamesVector[i].Real };
        }

        var orderedRecommendedGames = recommendedGames.OrderByDescending(rg => rg.Rating).ToArray();

        return orderedRecommendedGames ?? Array.Empty<RatedGame>();
    }

    /// <summary>
    /// Finds candidate games based on the features of input games.
    /// </summary>
    /// <param name="inputGames">An array of input games to used to determine the feature set.</param>
    /// <returns>An array of candidate games matching the features of the input games.</returns>
    public async Task<Game[]> FindCandidateGames(Game[] inputGames)
    {
        /*
        * This method retrieves candidate games from the IGDB API based on the
        * features (genres, themes, etc.) of the input games.
        * It constructs a query to filter games that share similar features with the input games, excluding
        * the input games themselves, and orders the results by rating in descending order.
        */

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
                & version_parent = null;
            sort rating desc;
            limit 500;
            ";
        var games = await _igdbClient.Query<Game[]>("games", query);


        return games ?? Array.Empty<Game>();
    }

    /// <summary>
    /// Creates a sparse matrix representing the feature set of the provided games.
    /// In this matrix each row corresponds to a game and each column a feature.
    /// </summary>
    /// <param name="games">An array of games for which the feature matrix is created.</param>
    /// <returns>A sparse matrix representing the feature set of the provided games.</returns>
    public static SparseMatrix CreateFeatureMatrix(Game[] games)
    {
        /*
        * This method constructs a sparse matrix representing the feature set of the provided games.
        * Each row of the matrix corresponds to a game, and each column corresponds to a unique feature
        * The presence of a feature in a game is indicated by setting the corresponding matrix element to 1.0.
        *
        * Note: The matrix is sparse, meaning that most of its elements are zero, as a game typically has
        * only a subset of the available features.
        */

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

    /// <summary>
    /// Generates a user profile vector based on the ratings of each rated games.
    /// </summary>
    /// <param name="ratedGames">An array of rated games used to generate the user profile vector.</param>
    /// <returns>A dense vector representing the user profile.</returns>
    public static DenseVector GenerateUserProfileVector(RatedGame[] ratedGames)
    {
        /*
        * This method generates a user profile vector based on the rated games.
        * The profile is essentially a list of features whose weights are determined by the
        * the weighted features of the given 'ratedGames'.
        *
        * The user profile represents the user's preferences across various features and is used in the recommendation
        * algorithm to suggest games with similar features.
        */

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

    /// <summary>
    /// Creates a feature set based on the aggregated features of a collection of games.
    /// </summary>
    /// <param name="games">An array of games from which to extract features for the feature set.</param>
    /// <returns>A feature set.</returns>
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

    /// <summary>
    /// Represents a set of features extracted from a collection of games.
    /// </summary>
    private class FeatureSet
    {
        public HashSet<int> Genres = new();
        public HashSet<int> Themes = new();
        public HashSet<int> PlayerPerspectives = new();
        public HashSet<int> GameModes = new();
        public HashSet<int> AgeRatings = new();
    }
}
