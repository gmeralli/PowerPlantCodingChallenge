using System.Text.Json.Serialization;
using static PowerPlantCodingChallenge.Common.Enums;

namespace PowerPlantCodingChallenge.Models
{
    public class PowerPlant
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        private string? _rawType;
        [JsonPropertyName("type")]
        public string? RawType 
        {
            get { return _rawType; }
            set
            {
                _rawType = value;
                if (Enum.TryParse(RawType, out PowerPlantTypes parsedPowerPlantType))
                {
                    ParsedType = parsedPowerPlantType;
                }
            }
        }

        public PowerPlantTypes? ParsedType { get; set; }

        [JsonPropertyName("efficiency")]
        public decimal? Efficiency { get; set; }

        [JsonPropertyName("pmin")]
        public decimal? PMin { get; set; }

        [JsonPropertyName("pmax")]
        public decimal? PMax { get; set; }

        public decimal? PerMWhCost { get; set; }

        public bool IsValid => !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(RawType) && Efficiency.HasValue && PMin.HasValue && PMax.HasValue;
    }
}
