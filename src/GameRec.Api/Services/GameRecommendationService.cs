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
}
