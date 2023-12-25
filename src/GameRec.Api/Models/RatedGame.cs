using System.Text.Json.Serialization;

namespace GameRec.Api.Models;

/// <summary>
/// Represents a rated game. Rating in this context is a weight, not a review rating.
/// </summary>
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
