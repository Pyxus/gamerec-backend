namespace GameRec.Api.Models;

public struct RatedGame
{
    public double Rating;
    public Game Game;

    public RatedGame(Game game, double rating)
    {
        Game = game;
        Rating = rating;
    }
}
