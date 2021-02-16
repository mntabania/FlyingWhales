﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;
namespace Locations.Region_Features {
    public class HauntedFeature : RegionFeature {
        public override void SpecialStructureGenerationSecondPassActions(Region region) {
            base.SpecialStructureGenerationSecondPassActions(region);
            //if there are less than 4 Ancient Ruins in the Region, add more so that it will have between 4 to 6 Ancient Ruins
            int existingRuins = GetAncientGraveyardsInRegion(region); 
            if (existingRuins < 4) {
                int missing = Random.Range(4, 7) - existingRuins;
                List<Area> choices = ObjectPoolManager.Instance.CreateNewAreaList();
                for (int i = 0; i < region.areas.Count; i++) {
                    Area currArea = region.areas[i];
                    if ((currArea.elevationType == ELEVATION.PLAIN || currArea.elevationType == ELEVATION.TREES) &&
                        currArea.structureComponent.HasStructureInArea() == false && //with no Features yet
                        !currArea.neighbourComponent.neighbours.Any( //and not adjacent to player Portal, Settlement or other non-cave landmarks
                            n => n.structureComponent.HasStructureInArea() && n.primaryStructureInArea.structureType.IsSettlementStructure())) {
                        choices.Add(currArea);
                    }
                }
                for (int i = 0; i < missing; i++) {
                    if (choices.Count == 0) { break; }
                    Area chosenTile = CollectionUtilities.GetRandomElement(choices);
                    choices.Remove(chosenTile);
                    MapGenerator.Instance.StartCoroutine(CreateSpecialStructure(STRUCTURE_TYPE.ANCIENT_GRAVEYARD, chosenTile.region, chosenTile));
                }
                ObjectPoolManager.Instance.ReturnAreaListToPool(choices);
            }
        }

        private int GetAncientGraveyardsInRegion(Region region) {
            return LandmarkManager.Instance.GetSpecialStructuresOfType(STRUCTURE_TYPE.ANCIENT_GRAVEYARD).Count;
        }

        private IEnumerator CreateSpecialStructure(STRUCTURE_TYPE p_structureType, Region p_region, Area p_area) {
            NPCSettlement settlement = LandmarkManager.Instance.CreateNewSettlement(p_region, LOCATION_TYPE.DUNGEON, p_area);
            yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.PlaceBuiltLandmark(settlement, p_region.innerMap, RESOURCE.NONE, p_structureType));
        }
    }
}