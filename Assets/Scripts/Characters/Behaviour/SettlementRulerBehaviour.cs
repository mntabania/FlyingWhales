using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;
using Inner_Maps.Location_Structures;
using Traits;

public class SettlementRulerBehaviour : CharacterBehaviourComponent {
    public SettlementRulerBehaviour() {
        priority = 22;
        attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY, BEHAVIOUR_COMPONENT_ATTRIBUTE.ONCE_PER_DAY };
    }

    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
#if DEBUG_LOG
        log += $"\n-{character.name} is a settlement ruler";
#endif
        NPCSettlement characterHomeSettlement = character.homeSettlement;
        if (characterHomeSettlement != null && characterHomeSettlement.prison != null) {
            if(character.faction != null) {
                LocationStructure structure = characterHomeSettlement.prison;
                int roll = Random.Range(0, 100);
#if DEBUG_LOG
                log += $"\n-15% chance to recruit a restrained character from different faction";
                log += $"\n-Roll: {roll}";
#endif
                if (roll < 15) {
                    Character targetCharacter = structure.GetRandomCharacterThatCanBeRecruitedBy(character);

                    if (targetCharacter != null) {
#if DEBUG_LOG
                        log += $"\n-Chosen target: {targetCharacter.name}";
#endif
                        return character.jobComponent.TriggerRecruitJob(targetCharacter, out producedJob);
                    }
                }
            }
            if (characterHomeSettlement.settlementType != null) {
                // int existingBuildJobs = character.homeSettlement.GetNumberOfJobsWith(JOB_TYPE.BUILD_BLUEPRINT);
                List<JobQueueItem> buildJobs = RuinarchListPool<JobQueueItem>.Claim();
                characterHomeSettlement.PopulateJobsOfType(buildJobs, JOB_TYPE.BUILD_BLUEPRINT);
                if (buildJobs.Count < 2) {
#if DEBUG_LOG
                    log += $"\n-Check chance to build dwelling if not yet at max.";
#endif
                    int dwellingCount = characterHomeSettlement.GetStructureCount(STRUCTURE_TYPE.DWELLING);
                    int totalDwellingCount = dwellingCount + GetJobsThatWillBuildDwelling(buildJobs);
                    if (totalDwellingCount < characterHomeSettlement.settlementType.maxDwellings) {
                        int chance = 3;
                        if (dwellingCount < (characterHomeSettlement.settlementType.maxDwellings/2)) {
                            chance = 5;
                        }
                        if (characterHomeSettlement.HasHomelessResident()) {
                            chance = 7;
                        }
                        if (GameUtilities.RollChance(chance, ref log)) {
#if DEBUG_LOG
                            log += $"\n-Chance met and dwellings not yet at maximum.";
#endif
                            //place dwelling blueprint
                            StructureSetting structureToPlace = characterHomeSettlement.settlementType.GetDwellingSetting(character.faction);
                            if (characterHomeSettlement.owner != null) {
                                structureToPlace = characterHomeSettlement.owner.factionType.ProcessStructureSetting(structureToPlace, characterHomeSettlement);    
                            }
                            if (LandmarkManager.Instance.CanPlaceStructureBlueprint(characterHomeSettlement, structureToPlace, out var targetTile, out var structurePrefabName, out var connectorToUse, out var connectorTile)) {
#if DEBUG_LOG
                                log += $"\n-Will place dwelling blueprint {structurePrefabName} at {targetTile}.";
#endif
                                RuinarchListPool<JobQueueItem>.Release(buildJobs);
                                return character.jobComponent.TriggerPlaceBlueprint(structurePrefabName, connectorToUse, structureToPlace, targetTile, connectorTile, out producedJob);    
                            }    
                        }
                    }
#if DEBUG_LOG
                    log += $"\n-Check chance to build a missing facility.";
#endif
                    int facilityCount = characterHomeSettlement.GetFacilityCount();
                    int totalFacilityCount = facilityCount + GetJobsThatWillBuildFacility(buildJobs);
                    if (totalFacilityCount < characterHomeSettlement.settlementType.maxFacilities) {
                        STRUCTURE_TYPE determinedStructureToUse = STRUCTURE_TYPE.NONE;
                        int chance = 2;
                        if (!characterHomeSettlement.HasStructure(STRUCTURE_TYPE.FISHERY) &&
                             !characterHomeSettlement.HasStructure(STRUCTURE_TYPE.FARM) &&
                             !characterHomeSettlement.HasStructure(STRUCTURE_TYPE.BUTCHERS_SHOP)) {
                            chance = 50;
#if DEBUG_LOG
                            log = $"{log}\n-{characterHomeSettlement.name} doesn't have a fishery, farm or butchers shop. Set chance to {chance}";
#endif
                            if (ShouldBuildFishery(characterHomeSettlement)) {
                                determinedStructureToUse = STRUCTURE_TYPE.FISHERY;
                            } else if (ShouldBuildButcher(characterHomeSettlement)) {
                                determinedStructureToUse = STRUCTURE_TYPE.FISHERY;  
                            } else {
                                determinedStructureToUse = STRUCTURE_TYPE.FARM;
                            }
#if DEBUG_LOG
                            log = $"{log}\n-Will try to build {determinedStructureToUse}";
#endif
                        } else if (!characterHomeSettlement.HasStructure(STRUCTURE_TYPE.LUMBERYARD) && characterHomeSettlement.owner != null && 
                                  characterHomeSettlement.owner.factionType.type == FACTION_TYPE.Elven_Kingdom && characterHomeSettlement.occupiedVillageSpot.HasUnusedLumberyardSpots()) {
                            //build lumberyard
                            chance = 50;
                            determinedStructureToUse = STRUCTURE_TYPE.LUMBERYARD;
#if DEBUG_LOG
                            log = $"{log}\n-{characterHomeSettlement.name} doesn't have a lumberyard and is owned by an elven kingdom and has unused lumberyard spots. Set chance to {chance} and will try to build {determinedStructureToUse}";
#endif
                        } else if (!characterHomeSettlement.HasStructure(STRUCTURE_TYPE.MINE) && characterHomeSettlement.owner != null && 
                                   characterHomeSettlement.owner.factionType.type == FACTION_TYPE.Human_Empire && characterHomeSettlement.occupiedVillageSpot.HasUnusedMiningSpots()) {
                            //build mine
                            chance = 50;
                            determinedStructureToUse = STRUCTURE_TYPE.MINE;
#if DEBUG_LOG
                            log = $"{log}\n-{characterHomeSettlement.name} doesn't have a mine and is owned by a Human Empire and has unused Mining spots. Set chance to {chance} and will try to build {determinedStructureToUse}";
#endif
                        } else if (!characterHomeSettlement.HasStructure(STRUCTURE_TYPE.LUMBERYARD) && characterHomeSettlement.occupiedVillageSpot.HasUnusedLumberyardSpots()) {
                            //build lumberyard
                            chance = 50;
                            determinedStructureToUse = STRUCTURE_TYPE.LUMBERYARD;  
#if DEBUG_LOG
                            log = $"{log}\n-{characterHomeSettlement.name} doesn't have a lumberyard and has unused lumberyard spots. Set chance to {chance} and will try to build {determinedStructureToUse}";
#endif
                        } else if (!characterHomeSettlement.HasStructure(STRUCTURE_TYPE.MINE) && characterHomeSettlement.occupiedVillageSpot.HasUnusedMiningSpots()) {
                            //build mine
                            chance = 50;
                            determinedStructureToUse = STRUCTURE_TYPE.MINE;  
#if DEBUG_LOG
                            log = $"{log}\n-{characterHomeSettlement.name} doesn't have a mine and has unused mining spots. Set chance to {chance} and will try to build {determinedStructureToUse}";
#endif
                        }

                        // chance = 100;
                        // determinedStructureToUse = STRUCTURE_TYPE.BUTCHERS_SHOP;
                        if (GameUtilities.RollChance(chance, ref log)) {
                            if (determinedStructureToUse == STRUCTURE_TYPE.NONE) {
#if DEBUG_LOG
                                log = $"{log}\n-Determined structure to build is none. Will determine now...";
#endif
                                int foodSupplyCapacity = characterHomeSettlement.resourcesComponent.GetFoodSupplyCapacity();
                                int resourceSupplyCapacity = characterHomeSettlement.resourcesComponent.GetResourceSupplyCapacity();
                                int villagerCount = characterHomeSettlement.residents.Count;
#if DEBUG_LOG
                                log = $"{log}\n-Food supply capacity: {foodSupplyCapacity.ToString()}. Resource Supply Capacity: {resourceSupplyCapacity.ToString()}. Villager Count: {villagerCount.ToString()}";
#endif
                                if (villagerCount > foodSupplyCapacity) {
#if DEBUG_LOG
                                    log = $"{log}\n-Villager count exceeds food supply capacity.";
#endif
                                    if (ShouldBuildFishery(characterHomeSettlement)) {
                                        determinedStructureToUse = STRUCTURE_TYPE.FISHERY;
                                    } else if (ShouldBuildButcher(characterHomeSettlement)) {
                                        determinedStructureToUse = STRUCTURE_TYPE.BUTCHERS_SHOP;
                                    } else {
                                        determinedStructureToUse = STRUCTURE_TYPE.FARM;
                                    }
#if DEBUG_LOG
                                    log = $"{log}\n-Will build {determinedStructureToUse}";
#endif
                                } else if (villagerCount > resourceSupplyCapacity) {
                                    bool hasUnusedLumberyardSpots = characterHomeSettlement.occupiedVillageSpot.HasUnusedLumberyardSpots();
                                    bool hasUnusedMiningSpots = characterHomeSettlement.occupiedVillageSpot.HasUnusedMiningSpots();
#if DEBUG_LOG
                                    log = $"{log}\n-Villager Count exceeds resource supply capacity. Has Unused Lumberyard Spots? {hasUnusedLumberyardSpots.ToString()}. Has Unused Mining Spots? {hasUnusedMiningSpots.ToString()}";
#endif
                                    if (characterHomeSettlement.owner != null && characterHomeSettlement.owner.factionType.type == FACTION_TYPE.Elven_Kingdom && hasUnusedLumberyardSpots) {
                                        determinedStructureToUse = STRUCTURE_TYPE.LUMBERYARD;
                                    } else if (characterHomeSettlement.owner != null && characterHomeSettlement.owner.factionType.type == FACTION_TYPE.Human_Empire && hasUnusedMiningSpots) {
                                        determinedStructureToUse = STRUCTURE_TYPE.MINE;
                                    } else if (hasUnusedLumberyardSpots) {
                                        determinedStructureToUse = STRUCTURE_TYPE.LUMBERYARD;
                                    } else if (hasUnusedMiningSpots) {
                                        determinedStructureToUse = STRUCTURE_TYPE.MINE;
                                    }
#if DEBUG_LOG
                                    log = $"{log}\n-Determined structure to build is {determinedStructureToUse.ToString()}";
#endif
                                }
                                if (determinedStructureToUse == STRUCTURE_TYPE.NONE) {
#if DEBUG_LOG
                                    log = $"{log}\n-Checking if should build skinners lodge...";
#endif
                                    if (ShouldBuildSkinnersLodge(characterHomeSettlement)) {
                                        determinedStructureToUse = STRUCTURE_TYPE.HUNTER_LODGE;
#if DEBUG_LOG
                                        log = $"{log}\n-Checking passed. Will try to build Skinners lodge";
#endif
                                    } else {
                                        int neededWorkShopCount = Mathf.CeilToInt((float)villagerCount / 8f);
                                        int workshopCount = characterHomeSettlement.GetStructureCount(STRUCTURE_TYPE.WORKSHOP);
#if DEBUG_LOG
                                        log = $"{log}\n-Will check if should build workshop. Needed workshops is {neededWorkShopCount.ToString()}. Current Workshop count is {workshopCount.ToString()}";
#endif
                                        if (workshopCount < neededWorkShopCount) {
                                            determinedStructureToUse = STRUCTURE_TYPE.WORKSHOP;
#if DEBUG_LOG
                                            log = $"{log}\n-Will try to build workshop";
#endif
                                        } else {
#if DEBUG_LOG
                                            log = $"{log}\n-Will check if should build tavern, hospice, prison or cemetery";
#endif
                                            if (!characterHomeSettlement.HasStructure(STRUCTURE_TYPE.TAVERN)) {
                                                determinedStructureToUse = STRUCTURE_TYPE.TAVERN;    
                                            } else if (!characterHomeSettlement.HasStructure(STRUCTURE_TYPE.HOSPICE)) {
                                                determinedStructureToUse = STRUCTURE_TYPE.HOSPICE;    
                                            } else if (!characterHomeSettlement.HasStructure(STRUCTURE_TYPE.PRISON)) {
                                                determinedStructureToUse = STRUCTURE_TYPE.PRISON;    
                                            } else if (!characterHomeSettlement.HasStructure(STRUCTURE_TYPE.CEMETERY)) {
                                                determinedStructureToUse = STRUCTURE_TYPE.CEMETERY;    
                                            }
#if DEBUG_LOG
                                            log = $"{log}\n-Will try to build {determinedStructureToUse.ToString()}";
#endif
                                        }
                                    }    
                                }
                            }
#if DEBUG_LOG
                            log = $"{log}\n-Final determined structure to build: {determinedStructureToUse}";
#endif
                            if (determinedStructureToUse != STRUCTURE_TYPE.NONE) {
                                return TryCreatePlaceBlueprintJob(determinedStructureToUse, character, characterHomeSettlement, out producedJob, ref log);
                            }
                        }
//                         int chance = 2;
//                         if (facilityCount < (character.homeSettlement.settlementType.maxFacilities/2)) {
//                             chance = 3;
//                         }
//                         if (GameUtilities.RollChance(chance, ref log)) {
// #if DEBUG_LOG
//                             log += $"\n-Chance to build facility met.";
// #endif
//                             //place random facility based on weights
//                             StructureSetting targetFacility = character.homeSettlement.GetMissingFacilityToBuildBasedOnWeights();
//                             if (character.homeSettlement.owner != null) {
//                                 targetFacility = character.homeSettlement.owner.factionType.ProcessStructureSetting(targetFacility, character.homeSettlement);    
//                             }
// #if DEBUG_LOG
//                             log += $"\n-Will try to build facility {targetFacility.ToString()}";
// #endif
//                             if (targetFacility.hasValue && LandmarkManager.Instance.CanPlaceStructureBlueprint(character.homeSettlement, targetFacility, out var targetTile, out var structurePrefabName, out var connectorToUse, out var connectorTile)) {
// #if DEBUG_LOG
//                                 log += $"\n-Will place blueprint {structurePrefabName} at {targetTile}.";
// #endif
//                                 RuinarchListPool<JobQueueItem>.Release(buildJobs);
//                                 return character.jobComponent.TriggerPlaceBlueprint(structurePrefabName, connectorToUse, targetFacility, targetTile, connectorTile, out producedJob);    
//                             } else {
// #if DEBUG_LOG
//                                 log += $"\n-Could not find location to place facility {targetFacility.ToString()}";
// #endif
//                             }
//                         }
                    }
                } else {
#if DEBUG_LOG
                    log += $"\n-Maximum build blueprint jobs reached.";
#endif
                }
                RuinarchListPool<JobQueueItem>.Release(buildJobs);
            }
        }
        producedJob = null;
        return false;
    }
    private int GetJobsThatWillBuildFacility(List<JobQueueItem> jobs) {
        int count = 0;
        for (int i = 0; i < jobs.Count; i++) {
            JobQueueItem job = jobs[i];
            if (job is GoapPlanJob goapPlanJob && goapPlanJob.poiTarget is GenericTileObject genericTileObject) {
                if (genericTileObject.blueprintOnTile != null && genericTileObject.blueprintOnTile.structureType.IsFacilityStructure()) {
                    count++;
                }
            }
        }
        return count;
    }
    private int GetJobsThatWillBuildDwelling(List<JobQueueItem> jobs) {
        int count = 0;
        for (int i = 0; i < jobs.Count; i++) {
            JobQueueItem job = jobs[i];
            if (job is GoapPlanJob goapPlanJob && goapPlanJob.poiTarget is GenericTileObject genericTileObject) {
                if (genericTileObject.blueprintOnTile != null && genericTileObject.blueprintOnTile.structureType == STRUCTURE_TYPE.DWELLING) {
                    count++;
                }
            }
        }
        return count;
    }
    private bool ShouldBuildFishery(NPCSettlement p_settlement) {
        if (p_settlement.owner != null && p_settlement.owner.factionType.IsActionConsideredACrime(CRIME_TYPE.Animal_Killing)) {
            //Animal Killing is considered a crime.
            return false;
        }
        if (!p_settlement.occupiedVillageSpot.HasUnusedFishingSpot()) {
            return false;
        }
        if (!p_settlement.HasResidentThatIsOrCanBecomeClass("Fisher")) {
            return false;
        }
        return true;
    }
    private bool ShouldBuildButcher(NPCSettlement p_settlement) {
        if (p_settlement.owner != null && p_settlement.owner.factionType.IsActionConsideredACrime(CRIME_TYPE.Animal_Killing)) {
            //Animal Killing is considered a crime.
            return false;
        }
        if (!p_settlement.occupiedVillageSpot.HasAccessToAnimals()) {
            return false;
        }
        if (!p_settlement.HasResidentThatIsOrCanBecomeClass("Butcher")) {
            return false;
        }
        return true;
    }
    private bool ShouldBuildSkinnersLodge(NPCSettlement p_settlement) {
        if (p_settlement.HasStructure(STRUCTURE_TYPE.HUNTER_LODGE)) {
            return false;
        }
        if (!p_settlement.occupiedVillageSpot.HasAccessToAnimals()) {
            return false;
        }
        if (!p_settlement.HasResidentThatIsOrCanBecomeClass("Skinner")) {
            return false;
        }
        return true;
    }

    private bool TryCreatePlaceBlueprintJob(STRUCTURE_TYPE p_structureType, Character character, NPCSettlement p_settlement, out JobQueueItem producedJob, ref string log) {
        StructureSetting structureSetting = character.faction.factionType.CreateStructureSettingForStructure(p_structureType, p_settlement);
#if DEBUG_LOG
        log = $"{log}\n-Will try to place blueprint {structureSetting.ToString()}";
#endif
        if (structureSetting.hasValue && LandmarkManager.Instance.CanPlaceStructureBlueprint(character.homeSettlement, structureSetting, out var targetTile, out var structurePrefabName, out var connectorToUse, out var connectorTile)) {
#if DEBUG_LOG
            log = $"{log}\n-Will place {structureSetting.ToString()} at {targetTile.ToString()} using template {structurePrefabName}";
#endif
            return character.jobComponent.TriggerPlaceBlueprint(structurePrefabName, connectorToUse, structureSetting, targetTile, connectorTile, out producedJob);    
        }
#if DEBUG_LOG
        log = $"{log}\n-Could not place blueprint {structureSetting.ToString()}";
#endif
        producedJob = null;
        return false;
    }
}
