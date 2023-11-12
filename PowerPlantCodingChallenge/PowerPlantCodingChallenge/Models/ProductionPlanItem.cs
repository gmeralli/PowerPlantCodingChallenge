using System.Text.Json.Serialization;

namespace PowerPlantCodingChallenge.Models
{
    public class ProductionPlanItem
    {
        [JsonIgnore]
        public PowerPlant? PowerPlant { get; set; }

        [JsonPropertyName("name")]
        public string? PowerPlantName => PowerPlant?.Name;

        [JsonPropertyName("p")]
        public decimal? PowerToDeliver { get; set; }
    }
}
