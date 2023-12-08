using System.Text.Json.Serialization;

namespace GameRec.Api.Models;

public struct RatedGame
{
    [JsonPropertyName("rating")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double Rating { get; set; }

    [JsonPropertyName("game")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Game Game { get; set; }

    public RatedGame() { }
}
