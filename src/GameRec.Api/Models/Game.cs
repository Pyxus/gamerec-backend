using System.Text.Json.Serialization;

namespace GameRec.Api.Models
{
    public struct Game
    {
        enum Genre
        {
            PointAndClick = 2,
            Fighting = 4,
            Shooter = 5,
            Music = 7,
            Platform = 8,
            Puzzle = 9,
            Racing = 10,
            RTS = 11,
            RPG = 12,
            Simulator = 13,
            Sport = 14,
            Strategy = 15,
            TBS = 16,
            Tactical = 24,
            HackNSlash = 25,
            Trivia = 26,
            Pinball = 30,
            Adventure = 31,
            Indie = 32,
            Arcade = 33,
            VisualNovel = 34,
            CardAndBoardGame = 35,
            MOBA = 36,
        }

        enum Theme
        {
            Action = 1,
            Fantasy = 17,
            SciFi = 18,
            Horror = 19,
            Thriller = 20,
            Survival = 21,
            Historical = 22,
            Stealth = 23,
            Comedy = 27,
            Business = 28,
            Drama = 31,
            NonFiction = 32,
            Sandbox = 33,
            Educational = 34,
            Kids = 35,
            OpenWorld = 38,
            Warfare = 39,
            Party = 40,
            FourX = 41,
            Erotic = 42,
            Mystery = 43,
            Romance = 44,
        }

        enum PlayerPerspective
        {
            FirstPerson = 1,
            ThirdPerson,
            Isometric,
            SideView,
            Text,
            Auditory,
            VR,
        }

        enum GameMode
        {
            SinglePlayer = 1,
            Multiplayer,
            Cooperative,
            SplitScreen,
            MMO,
            BattleRoyale,
        }

        enum AgeRating
        {
            Three = 1,
            Seven,
            Twelve,
            Sixteen,
            Eighteen,
            RP,
            EC,
            E,
            E10,
            T,
            M,
            AO,
            CERO_A,
            CERO_B,
            CERO_C,
            CERO_D,
            CERO_Z,
            USK_0,
            USK_6,
            USK_12,
            USK_16,
            USK_18,
            GRAC_ALL,
            GRAC_Fifteen,
            GRAC_Eighteen,
            GRAC_TESTING,
            CLASS_IND_L,
            CLASS_IND_Ten,
            CLASS_IND_Twelve,
            CLASS_IND_Fourteen,
            CLASS_IND_Sixteen,
            CLASS_IND_Eighteen,
            ACB_G,
            ACB_PG,
            ACB_M,
            ACB_MA15,
            ACB_R18,
            ACB_RC
        }

        [JsonPropertyName("id")]
        public int Id { get; set; } = 0;

        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string Name { get; set; } = "";

        [JsonPropertyName("genres")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int[] Genres { get; set; } = Array.Empty<int>();

        [JsonPropertyName("themes")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int[] Themes { get; set; } = Array.Empty<int>();

        [JsonPropertyName("player_perspectives")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int[] PlayerPerspectives { get; set; } = Array.Empty<int>();

        [JsonPropertyName("game_modes")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int[] GameModes { get; set; } = Array.Empty<int>();

        [JsonPropertyName("age_ratings")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int[] AgeRatings { get; set; } = Array.Empty<int>();

        public Game()
        {
        }
    }
}