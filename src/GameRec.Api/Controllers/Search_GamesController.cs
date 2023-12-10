using GameRec.Api.Clients;
using GameRec.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace GameRec.Api.Controllers
{
    [Route("[controller]")]
    public class Search_GamesController : Controller
    {
        private readonly IgdbClient _igdbClient;

        public Search_GamesController(IgdbClient igdbClient)
        {
            _igdbClient = igdbClient;
        }


        [HttpGet]
        public async Task<Game[]> Get([FromQuery] string name)
        {
            Console.WriteLine(name);
            return await _igdbClient.SearchForGame(name);
        }
    }
}