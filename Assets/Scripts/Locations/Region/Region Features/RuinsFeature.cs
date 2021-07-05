using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityScripts;
namespace Locations.Region_Features {
    public class RuinsFeature : RegionFeature {
        public override void SpecialStructureGenerationSecondPassActions(Region region) {
            base.SpecialStructureGenerationSecondPassActions(region);
            //if there are less than 4 Ancient Ruins in the Region, add more so that it will have between 4 to 6 Ancient Ruins
            int existingRuins = GetAncientRuinsInRegion(region); 
            if (existingRuins < 4) {
                int missing = Random.Range(4, 7) - existingRuins;
                List<Area> choices = ObjectPoolManager.Instance.CreateNewAreaList();
                for (int i = 0; i < region.areas.Count; i++) {
                    Area currArea = region.areas[i];
                    if (currArea.elevationComponent.IsFully(ELEVATION.PLAIN) &&
                        currArea.GetOccupyingVillageSpot() == null &&
                                currArea.structureComponent.HasStructureInArea() == false && //with no Features yet
                                !currArea.neighbourComponent.neighbours.Any( //and not adjacent to player Portal, Settlement or other non-cave landmarks
                                    n => n.structureComponent.HasStructureInArea() && n.primaryStructureInArea.structureType.IsVillageStructure())) {
                        choices.Add(currArea);
                    }
                }
                for (int i = 0; i < missing; i++) {
                    if (choices.Count == 0) { break; }
                    Area chosenTile = CollectionUtilities.GetRandomElement(choices);
                    choices.Remove(chosenTile);
                    MapGenerator.Instance.StartCoroutine(CreateSpecialStructure(STRUCTURE_TYPE.ANCIENT_RUIN, chosenTile.region, chosenTile));
                }
                ObjectPoolManager.Instance.ReturnAreaListToPool(choices);
            }
        }

        private int GetAncientRuinsInRegion(Region region) {
            return LandmarkManager.Instance.GetStructuresOfTypeCount(STRUCTURE_TYPE.ANCIENT_RUIN);
        }
        private IEnumerator CreateSpecialStructure(STRUCTURE_TYPE p_structureType, Region p_region, Area p_area) {
            NPCSettlement settlement = LandmarkManager.Instance.CreateNewSettlement(p_region, LOCATION_TYPE.DUNGEON, p_area);
            yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.PlaceBuiltLandmark(settlement, p_region.innerMap, RESOURCE.NONE, p_structureType));
        }
    }
}