namespace PowerPlantCodingChallenge.Models
{
    public class Payload
    {
        public decimal? Load { get; set; }

        public Fuels? Fuels { get; set; }

        public IEnumerable<PowerPlant>? PowerPlants { get; set; }

        public bool IsValid => Load.HasValue && Fuels != null && PowerPlants != null;
    }
}
