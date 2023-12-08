using GameRec.Api.Clients;
using GameRec.Api.Models;
using GameRec.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace gamerec.Controllers;

[ApiController]
[Route("[controller]")]
public class GameRecommendationController
{
    private readonly IgdbClient _igdbClient;
    public GameRecommendationController(IgdbClient igdbClient)
    {
        _igdbClient = igdbClient;
    }

    [HttpPost(Name = "game_recommendations")]
    public async Task<RatedGame[]> Post([FromBody] Dictionary<int, double> ratingByIds)
    {

        var gameRec = new GameRecommendationService(_igdbClient);
        return await gameRec.GetRecommendedGames(ratingByIds);
    }
}
