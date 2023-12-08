namespace GameRec.Api.Models;

public struct RatedGame
{
    public float Rating;
    public Game Game;

    public RatedGame(Game game, float rating)
    {
        Game = game;
        Rating = rating;
    }
}
