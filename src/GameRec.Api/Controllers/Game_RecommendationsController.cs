using GameRec.Api.Clients;
using GameRec.Api.Models;
using GameRec.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace gamerec.Controllers;

[ApiController]
[Route("[controller]")]
public class Game_RecommendationsController
{
    private readonly IgdbClient _igdbClient;
    public Game_RecommendationsController(IgdbClient igdbClient)
    {
        _igdbClient = igdbClient;
    }

    [HttpPost]
    public async Task<RatedGame[]> Post([FromBody] Dictionary<int, double> ratingByIds)
    {
        var gameRec = new GameRecommendationService(_igdbClient);
        return await gameRec.GetRecommendedGames(ratingByIds);
    }
}
