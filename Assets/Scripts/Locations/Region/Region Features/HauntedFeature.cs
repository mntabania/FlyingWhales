using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityScripts;
namespace Locations.Region_Features {
    public class HauntedFeature : RegionFeature {
        public override void LandmarkGenerationSecondPassActions(Region region) {
            base.LandmarkGenerationSecondPassActions(region);
            //if there are less than 4 Ancient Ruins in the Region, add more so that it will have between 4 to 6 Ancient Ruins
            int existingRuins = GetAncientGraveyardsInRegion(region); 
            if (existingRuins < 4) {
                int missing = Random.Range(4, 7) - existingRuins;
                List<Area> choices = ObjectPoolManager.Instance.CreateNewAreaList();
                for (int i = 0; i < region.areas.Count; i++) {
                    Area currArea = region.areas[i];
                    if ((currArea.elevationType == ELEVATION.PLAIN || currArea.elevationType == ELEVATION.TREES) &&
                                currArea.structureComponent.HasStructureInArea() == false && //with no Features yet
                                currArea.neighbourComponent.neighbours.Any( //and not adjacent to player Portal, Settlement or other non-cave landmarks
                                    n => n.structureComponent.HasStructureInArea() &&
                                         (n.structureComponent.structures[0].structureType == STRUCTURE_TYPE.THE_PORTAL ||
                                          n.structureComponent.structures[0].structureType.IsSettlementStructure())) == false) {
                        choices.Add(currArea);
                    }
                }
                //List<HexTile> choices = region.areas
                //    .Where(x => (x.elevationType == ELEVATION.PLAIN || x.elevationType == ELEVATION.TREES) &&
                //                x.landmarkOnTile == null &&
                //                x.neighbourComponent.neighbours.Any( //and not adjacent to player Portal, Settlement or other non-cave landmarks
                //                    n => n.landmarkOnTile != null && 
                //                         (n.landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.THE_PORTAL ||
                //                          n.landmarkOnTile.specificLandmarkType.GetStructureType().IsSettlementStructure())) == false
                //    )
                //    .ToList();
                for (int i = 0; i < missing; i++) {
                    if (choices.Count == 0) { break; }
                    Area chosenArea = CollectionUtilities.GetRandomElement(choices);
                    //LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenArea, LANDMARK_TYPE.ANCIENT_GRAVEYARD);
                    LandmarkManager.Instance.CreateNewSettlement(chosenArea.region, LOCATION_TYPE.DUNGEON, chosenArea);
                    choices.Remove(chosenArea);
                }
                ObjectPoolManager.Instance.ReturnAreaListToPool(choices);
            }
        }

        private int GetAncientGraveyardsInRegion(Region region) {
            int count = 0;
            for (int i = 0; i < LandmarkManager.Instance.allLandmarks.Count; i++) {
                BaseLandmark landmark = LandmarkManager.Instance.allLandmarks[i];
                if (landmark.specificLandmarkType == LANDMARK_TYPE.ANCIENT_GRAVEYARD && 
                    landmark.tileLocation.region == region) {
                    count++;
                }
            }
            return count;
        }
    }
}