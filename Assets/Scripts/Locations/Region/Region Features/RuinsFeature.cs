﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityScripts;
namespace Locations.Region_Features {
    public class RuinsFeature : RegionFeature {
        public override void LandmarkGenerationSecondPassActions(Region region) {
            base.LandmarkGenerationSecondPassActions(region);
            //if there are less than 4 Ancient Ruins in the Region, add more so that it will have between 4 to 6 Ancient Ruins
            int existingRuins = GetAncientRuinsInRegion(region); 
            if (existingRuins < 4) {
                int missing = Random.Range(4, 7) - existingRuins;
                List<HexTile> choices = region.areas
                    .Where(x => (x.elevationType == ELEVATION.PLAIN || x.elevationType == ELEVATION.TREES) &&
                                x.landmarkOnTile == null && //with no Features yet
                                x.neighbourComponent.neighbours.Any( //and not adjacent to player Portal, Settlement or other non-cave landmarks
                                    n => n.landmarkOnTile != null && 
                                         (n.landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.THE_PORTAL ||
                                          n.landmarkOnTile.specificLandmarkType.GetStructureType().IsSettlementStructure())) == false
                    )
                    .ToList();
                for (int i = 0; i < missing; i++) {
                    if (choices.Count == 0) { break; }
                    HexTile chosenTile = CollectionUtilities.GetRandomElement(choices);
                    LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenTile, LANDMARK_TYPE.ANCIENT_RUIN);
                    LandmarkManager.Instance.CreateNewSettlement(chosenTile.region, LOCATION_TYPE.DUNGEON, chosenTile);
                    choices.Remove(chosenTile);
                }
            }
        }

        private int GetAncientRuinsInRegion(Region region) {
            int count = 0;
            for (int i = 0; i < LandmarkManager.Instance.allLandmarks.Count; i++) {
                BaseLandmark landmark = LandmarkManager.Instance.allLandmarks[i];
                if (landmark.specificLandmarkType == LANDMARK_TYPE.ANCIENT_RUIN && 
                    landmark.tileLocation.region == region) {
                    count++;
                }
            }
            return count;
        }
    }
}