using System.Collections;
using System.Collections.Generic;

using System.Linq;
using Inner_Maps.Location_Structures;
using Locations.Area_Features;
using Scenario_Maps;
using UnityEngine;
using UtilityScripts;
namespace Generator.Map_Generation.Components {
    public class SettlementFinalization : MapGenerationComponent {
        public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
            // for (int i = 0; i < LandmarkManager.Instance.allNonPlayerSettlements.Count; i++) {
            //     NPCSettlement npcSettlement = LandmarkManager.Instance.allNonPlayerSettlements[i];
            //     Faction faction = npcSettlement.owner;
            //     if (npcSettlement.locationType == LOCATION_TYPE.VILLAGE && !npcSettlement.HasFoodProducingStructure()) {
            //         StructureSetting foodProducingStructure = GenerateFoodProducingStructure(npcSettlement, faction);
            //         ProcessBeforePlacingFoodProducingStructure(foodProducingStructure, npcSettlement);
            //         yield return MapGenerator.Instance.StartCoroutine(VillageGeneration.PlaceStructure(npcSettlement.region, foodProducingStructure, npcSettlement));
            //         if (!npcSettlement.HasStructure(foodProducingStructure.structureType)) {
            //             //if structure was not placed, then just place a farm
            //             yield return MapGenerator.Instance.StartCoroutine(VillageGeneration.PlaceStructure(npcSettlement.region, new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.NONE, faction.factionType.usesCorruptedStructures), npcSettlement));    
            //         }
            //     }
            //     yield return null;
            // }
            yield return null;
        }

        #region Scenario Maps
        public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
            yield return MapGenerator.Instance.StartCoroutine(ExecuteRandomGeneration(data));
        }
        #endregion

        private StructureSetting GenerateFoodProducingStructure(NPCSettlement settlement, Faction faction) {
            WeightedDictionary<StructureSetting> choices = new WeightedDictionary<StructureSetting>();
            //Added checking since there are no corrupted fishing shacks/Hunters lodge yet.
            if (settlement.occupiedVillageSpot.reservedAreas.Count(t => t.elevationType == ELEVATION.WATER) > 0) {
                choices.AddElement(new StructureSetting(STRUCTURE_TYPE.FISHERY, RESOURCE.WOOD, faction.factionType.usesCorruptedStructures), 200);
            }
            choices.AddElement(new StructureSetting(STRUCTURE_TYPE.HUNTER_LODGE, faction.factionType.mainResource, faction.factionType.usesCorruptedStructures), 20);    
            choices.AddElement(new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.NONE, faction.factionType.usesCorruptedStructures), 20);
            return choices.PickRandomElementGivenWeights();
        }
        private void ProcessBeforePlacingFoodProducingStructure(StructureSetting p_foodProducingStructure, NPCSettlement p_settlement) {
            List<Area> surroundingAreas = ObjectPoolManager.Instance.CreateNewAreaList();
            p_settlement.PopulateSurroundingAreas(surroundingAreas);
            Area surroundingArea = CollectionUtilities.GetRandomElement(surroundingAreas);
            if (p_foodProducingStructure.structureType == STRUCTURE_TYPE.HUNTER_LODGE) {
                //add game feature to a surrounding area
                surroundingArea.featureComponent.AddFeature(AreaFeatureDB.Game_Feature, surroundingArea);
            }
            ObjectPoolManager.Instance.ReturnAreaListToPool(surroundingAreas);
        }

    }
}