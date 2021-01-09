using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

namespace UtilityScripts {
    public static class JobUtilities {
        public static void PopulatePriorityLocationsForHappinessRecovery(Character actor, GoapPlanJob job) {
            if (actor.homeStructure != null) {
                job.AddPriorityLocation(INTERACTION_TYPE.NONE, actor.homeStructure);

                LocationStructure currentStructure = actor.currentStructure;
                ILocation currentLocation = currentStructure;
                if (currentStructure == null || currentStructure.structureType == STRUCTURE_TYPE.WILDERNESS || currentStructure.structureType == STRUCTURE_TYPE.OCEAN) {
                    if (actor.gridTileLocation != null && actor.gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
                        currentLocation = actor.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner;
                    }
                }
                if (currentLocation != null) {
                    job.AddPriorityLocation(INTERACTION_TYPE.NONE, currentLocation);
                }
            }
        }
        public static void PopulatePriorityLocationsForTakingNonEdibleResources(Character actor, GoapPlanJob job, INTERACTION_TYPE actionType) {
            NPCSettlement homeSettlement = actor.homeSettlement;
            LocationStructure homeStructure = actor.homeStructure;
            if (homeStructure != null) {
                job.AddPriorityLocation(actionType, homeStructure);
            }
            if (homeSettlement != null) {
                LocationStructure cityCenter = homeSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
                LocationStructure lumberyard = homeSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.LUMBERYARD);
                if (cityCenter != null) {
                    job.AddPriorityLocation(actionType, cityCenter);
                }
                if (lumberyard != null) {
                    job.AddPriorityLocation(actionType, lumberyard);
                }
            }
        }
    }
}