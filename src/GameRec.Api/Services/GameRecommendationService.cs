using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameRec.Api.Clients;

namespace GameRec.Api.Services
{
    public class GameRecommendationService
    {
        private readonly IgdbClient _igdb;
        public GameRecommendationService(IgdbClient igdb)
        {
            _igdb = igdb;
        }
    }
}