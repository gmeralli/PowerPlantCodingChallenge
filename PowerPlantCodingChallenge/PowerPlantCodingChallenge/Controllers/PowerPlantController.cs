using Microsoft.AspNetCore.Mvc;
using PowerPlantCodingChallenge.Common;
using PowerPlantCodingChallenge.Models;
using static PowerPlantCodingChallenge.Common.Enums;

namespace PowerPlantCodingChallenge.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PowerPlantController : ControllerBase
    {
        /// <summary>
        /// Returns the production plan for the payload contained in the body of the POST request sent to the productionplan endpoint
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpPost(Name = "productionplan")]        
        public IEnumerable<ProductionPlanItem> CalculateProductionPlan([FromBody] Payload payload)
        {
            if (!payload.IsValid) return new List<ProductionPlanItem>();

            var validPowerPlants = payload.PowerPlants.Where(p => p.IsValid).ToList();

            //We compare the costs of the valid powerplants, to sort them by merit order
            foreach (PowerPlant powerPlant in validPowerPlants)
            {
                SetPerMWhCost(payload, powerPlant);
            }

            return GenerateProductionPlanResponse(payload, validPowerPlants);
        }

        /// <summary>
        /// Computes the cost of generating 1 MWh with the specified power plant
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="powerPlant"></param>
        /// <returns></returns>
        private void SetPerMWhCost(Payload payload, PowerPlant powerPlant)
        {
            if (payload == null || payload.Fuels == null || powerPlant == null || powerPlant.ParsedType == null || (powerPlant.Efficiency ?? 0) == 0) return;

            if (powerPlant.ParsedType == PowerPlantTypes.gasfired)
            {
                //Gas fired energy cost + emission allowance cost
                powerPlant.PerMWhCost = (payload.Fuels.Gas / powerPlant.Efficiency) + (Constants.CO2TonPerMWh * payload.Fuels.CO2);
            }
            else if (powerPlant.ParsedType == PowerPlantTypes.turbojet)
            {
                powerPlant.PerMWhCost = payload.Fuels.Kerosine / powerPlant.Efficiency;                
            }
            else if (powerPlant.ParsedType == PowerPlantTypes.windturbine)
            {
                powerPlant.PerMWhCost = 0;
                //The wind turbine's PMax depends on the wind percentage
                powerPlant.PMax = payload.Fuels.Wind / 100 * powerPlant.PMax;
            }
        }

        /// <summary>
        /// Returns the production plan for the specified payload, using the specified powerplants
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="powerPlants"></param>
        /// <returns></returns>
        private IEnumerable<ProductionPlanItem> GenerateProductionPlanResponse(Payload payload, IEnumerable<PowerPlant> powerPlants)
        {
            var productionPlanItems = new List<ProductionPlanItem>();
            var load = payload.Load;
            var meritOrderedPowerPlants = powerPlants.OrderBy(p => p.PerMWhCost);

            foreach (PowerPlant powerPlant in meritOrderedPowerPlants)
            {
                var productionPlanItem = new ProductionPlanItem() { PowerPlant = powerPlant };
                var pmin = powerPlant.PMin;
                var pmax = powerPlant.PMax;

                if (load >= pmax)
                {
                    productionPlanItem.PowerToDeliver = pmax;
                }
                else if (load > pmin)
                {
                    productionPlanItem.PowerToDeliver = load;
                }
                else if (load > 0)
                {
                    productionPlanItem.PowerToDeliver = pmin;
                }
                else productionPlanItem.PowerToDeliver = 0;

                productionPlanItems.Add(productionPlanItem);
                
                //We compute the remaining load to produce
                load -= pmax;
            }

            //TODO : display an error if load > 0 (means that the required load could not be produced by the provided power plants)

            return productionPlanItems;
        }        
    }
}
