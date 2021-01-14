using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

namespace UtilityScripts {
    public static class JobUtilities {
        public static void PopulatePriorityLocationsForHappinessRecovery(Character actor, GoapPlanJob job) {
            if (!actor.traitContainer.HasTrait("Travelling")) {
                bool hasPrioLocation = false;
                if(actor.homeStructure != null) {
                    job.AddPriorityLocation(INTERACTION_TYPE.NONE, actor.homeStructure);
                    hasPrioLocation = true;
                }
                if(actor.homeSettlement != null) {
                    LocationStructure tavern = actor.homeSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.TAVERN);
                    if(tavern != null) {
                        job.AddPriorityLocation(INTERACTION_TYPE.NONE, tavern);
                        hasPrioLocation = true;
                    }
                }

                if (hasPrioLocation) {
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
        public static void PopulatePriorityLocationsForTakingNonEdibleResources(NPCSettlement settlement, GoapPlanJob job, INTERACTION_TYPE actionType) {
            if (settlement != null) {
                LocationStructure cityCenter = settlement.GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
                LocationStructure lumberyard = settlement.GetFirstStructureOfType(STRUCTURE_TYPE.LUMBERYARD);
                if (cityCenter != null) {
                    job.AddPriorityLocation(actionType, cityCenter);
                }
                if (lumberyard != null) {
                    job.AddPriorityLocation(actionType, lumberyard);
                }
            }
        }
        public static void PopulatePriorityLocationsForTakingEdibleResources(Character actor, GoapPlanJob job, INTERACTION_TYPE actionType) {
            NPCSettlement homeSettlement = actor.homeSettlement;
            LocationStructure homeStructure = actor.homeStructure;
            if (homeStructure != null) {
                job.AddPriorityLocation(actionType, homeStructure);
            }
            if (homeSettlement != null) {
                LocationStructure cityCenter = homeSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
                if (cityCenter != null) {
                    job.AddPriorityLocation(actionType, cityCenter);
                }
            }
        }
        public static void PopulatePriorityLocationsForTakingPersonalItem(Character actor, GoapPlanJob job, INTERACTION_TYPE actionType) {
            NPCSettlement homeSettlement = actor.homeSettlement;
            if (homeSettlement != null) {
                LocationStructure cityCenter = homeSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
                if (cityCenter != null) {
                    job.AddPriorityLocation(actionType, cityCenter);
                }
            }
        }
        public static void PopulatePriorityLocationsForTakingPersonalItem(NPCSettlement settlement, GoapPlanJob job, INTERACTION_TYPE actionType) {
            if (settlement != null) {
                LocationStructure cityCenter = settlement.GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
                if (cityCenter != null) {
                    job.AddPriorityLocation(actionType, cityCenter);
                }
            }
        }
        public static void PopulatePriorityLocationsForSuicide(Character actor, GoapPlanJob job) {
            NPCSettlement homeSettlement = actor.homeSettlement;
            if (homeSettlement != null) {
                LocationStructure cityCenter = homeSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
                
                if(cityCenter != null) {
                    job.AddPriorityLocation(INTERACTION_TYPE.NONE, cityCenter);

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
        }

        #region Produce Resources
        public static void PopulatePriorityLocationsForProduceResources(NPCSettlement settlement, GoapPlanJob job, RESOURCE resourceType) {
            if (settlement != null) {
                List<LocationStructure> priorityStructures = ObjectPoolManager.Instance.CreateNewStructuresList();
                switch (resourceType) {
                    case RESOURCE.FOOD:
                        PopulatePriorityLocationsForProduceFood(settlement, job);
                        break;
                    case RESOURCE.WOOD:
                        PopulatePriorityLocationsForProduceWood(settlement, job);
                        break;
                    case RESOURCE.STONE:
                        PopulatePriorityLocationsForProduceStone(settlement, job);
                        break;
                    case RESOURCE.METAL:
                        PopulatePriorityLocationsForProduceMetal(settlement, job);
                        break;
                }
            }
        }
        private static void PopulatePriorityLocationsForProduceFood(NPCSettlement settlement, GoapPlanJob job) {
            List<LocationStructure> farms = settlement.GetStructuresOfType(STRUCTURE_TYPE.FARM);
            List<LocationStructure> fishingShacks = settlement.GetStructuresOfType(STRUCTURE_TYPE.FISHING_SHACK);
            List<LocationStructure> hunterLodge = settlement.GetStructuresOfType(STRUCTURE_TYPE.HUNTER_LODGE);

            if (farms != null) {
                for (int i = 0; i < farms.Count; i++) {
                    job.AddPriorityLocation(INTERACTION_TYPE.NONE, farms[i]);
                }
            }
            if (fishingShacks != null) {
                for (int i = 0; i < fishingShacks.Count; i++) {
                    FishingShack fishingShack = fishingShacks[i] as FishingShack;
                    job.AddPriorityLocation(INTERACTION_TYPE.NONE, fishingShack.connectedOcean);
                }
            }
            if (hunterLodge != null) {
                for (int i = 0; i < hunterLodge.Count; i++) {
                    job.AddPriorityLocation(INTERACTION_TYPE.NONE, hunterLodge[i]);
                    job.AddPriorityLocation(INTERACTION_TYPE.NONE, hunterLodge[i].occupiedHexTile.hexTileOwner);
                }
            }
        }
        private static void PopulatePriorityLocationsForProduceWood(NPCSettlement settlement, GoapPlanJob job) {
            List<LocationStructure> lumberyards = settlement.GetStructuresOfType(STRUCTURE_TYPE.LUMBERYARD);

            if (lumberyards != null) {
                for (int i = 0; i < lumberyards.Count; i++) {
                    job.AddPriorityLocation(INTERACTION_TYPE.NONE, lumberyards[i]);
                    job.AddPriorityLocation(INTERACTION_TYPE.NONE, lumberyards[i].occupiedHexTile.hexTileOwner);
                }
            }
        }
        private static void PopulatePriorityLocationsForProduceStone(NPCSettlement settlement, GoapPlanJob job) {
            List<LocationStructure> quarries = settlement.GetStructuresOfType(STRUCTURE_TYPE.QUARRY);

            if (quarries != null) {
                for (int i = 0; i < quarries.Count; i++) {
                    job.AddPriorityLocation(INTERACTION_TYPE.NONE, quarries[i]);
                    job.AddPriorityLocation(INTERACTION_TYPE.NONE, quarries[i].occupiedHexTile.hexTileOwner);
                }
            }
        }
        private static void PopulatePriorityLocationsForProduceMetal(NPCSettlement settlement, GoapPlanJob job) {
            List<LocationStructure> mineShacks = settlement.GetStructuresOfType(STRUCTURE_TYPE.MINE_SHACK);

            if (mineShacks != null) {
                for (int i = 0; i < mineShacks.Count; i++) {
                    MineShack mineShack = mineShacks[i] as MineShack;
                    job.AddPriorityLocation(INTERACTION_TYPE.NONE, mineShack.connectedCave);
                }
            }
        }
        #endregion
    }
}