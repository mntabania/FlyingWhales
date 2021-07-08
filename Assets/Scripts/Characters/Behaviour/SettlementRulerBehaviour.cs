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
        // attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY, BEHAVIOUR_COMPONENT_ATTRIBUTE.ONCE_PER_DAY };
    }

    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        if (character.partyComponent.isActiveMember) {
            producedJob = null;
            return false;
        }
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
                    int totalDwellingBlueprintJobs = GetJobsThatWillBuildDwelling(buildJobs);
                    int totalDwellingCount = dwellingCount + totalDwellingBlueprintJobs;
                    if (totalDwellingBlueprintJobs <= 0 && totalDwellingCount < characterHomeSettlement.settlementType.maxDwellings) {
                        int chance = 3;
                        if (dwellingCount < (characterHomeSettlement.settlementType.maxDwellings/2)) {
                            chance = 5;
                        }
                        if (characterHomeSettlement.HasHomelessResident()) {
                            chance = 7;
                        }

                        // chance = 100;
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
                    int totalFacilityBlueprintJobs = GetJobsThatWillBuildFacility(buildJobs);
                    int totalFacilityCount = facilityCount + totalFacilityBlueprintJobs;
                    if (totalFacilityBlueprintJobs <= 0 && totalFacilityCount < characterHomeSettlement.settlementType.maxFacilities) {
                        STRUCTURE_TYPE determinedStructureToUse = STRUCTURE_TYPE.NONE;
                        int chance = ChanceData.GetChance(CHANCE_TYPE.Settlement_Ruler_Default_Facility_Chance);
                        List<string> ableClassesOfAllResidents = RuinarchListPool<string>.Claim();
                        characterHomeSettlement.PopulateAbleClassesOfAllResidents(ableClassesOfAllResidents);
                        
                        if (!HasActiveWorkStructureOfType(characterHomeSettlement, STRUCTURE_TYPE.FISHERY, "Fisher", ableClassesOfAllResidents)
                             && !HasActiveWorkStructureOfType(characterHomeSettlement, STRUCTURE_TYPE.FARM, "Farmer", ableClassesOfAllResidents)
                             && !HasActiveWorkStructureOfType(characterHomeSettlement, STRUCTURE_TYPE.BUTCHERS_SHOP, "Butcher", ableClassesOfAllResidents)) {
                            chance = 50;
#if DEBUG_LOG
                            log = $"{log}\n-{characterHomeSettlement.name} doesn't have a fishery, farm or butchers shop. Set chance to {chance}";
#endif
                            if (ShouldBuildFishery(characterHomeSettlement, ableClassesOfAllResidents)) {
                                determinedStructureToUse = STRUCTURE_TYPE.FISHERY;
                            } else if (ShouldBuildButcher(characterHomeSettlement, ableClassesOfAllResidents)) {
                                determinedStructureToUse = STRUCTURE_TYPE.BUTCHERS_SHOP;  
                            } else {
                                determinedStructureToUse = STRUCTURE_TYPE.FARM;
                            }
#if DEBUG_LOG
                            log = $"{log}\n-Will try to build {determinedStructureToUse}";
#endif
                        } else if (characterHomeSettlement.owner != null && characterHomeSettlement.owner.factionType.type == FACTION_TYPE.Elven_Kingdom &&
                                   !characterHomeSettlement.HasStructure(STRUCTURE_TYPE.LUMBERYARD) && !characterHomeSettlement.HasBlueprintOnTileForStructure(STRUCTURE_TYPE.LUMBERYARD)) {
                            //build lumberyard
                            chance = 50;
                            determinedStructureToUse = STRUCTURE_TYPE.LUMBERYARD;
#if DEBUG_LOG
                            log = $"{log}\n-{characterHomeSettlement.name} doesn't have a lumberyard and is owned by an elven kingdom and has unused lumberyard spots. Set chance to {chance} and will try to build {determinedStructureToUse}";
#endif
                        } else if (characterHomeSettlement.owner != null && characterHomeSettlement.owner.factionType.type == FACTION_TYPE.Human_Empire &&
                                   !characterHomeSettlement.HasStructure(STRUCTURE_TYPE.MINE) && !characterHomeSettlement.HasBlueprintOnTileForStructure(STRUCTURE_TYPE.MINE) &&
                                   characterHomeSettlement.occupiedVillageSpot.HasUnusedMiningSpots()) {
                            //build mine
                            chance = 50;
                            determinedStructureToUse = STRUCTURE_TYPE.MINE;
#if DEBUG_LOG
                            log = $"{log}\n-{characterHomeSettlement.name} doesn't have a mine and is owned by a Human Empire and has unused Mining spots. Set chance to {chance} and will try to build {determinedStructureToUse}";
#endif
                        } else if (!characterHomeSettlement.HasStructure(STRUCTURE_TYPE.MINE) && !characterHomeSettlement.HasBlueprintOnTileForStructure(STRUCTURE_TYPE.MINE) &&
                                   !characterHomeSettlement.HasStructure(STRUCTURE_TYPE.LUMBERYARD) && !characterHomeSettlement.HasBlueprintOnTileForStructure(STRUCTURE_TYPE.LUMBERYARD)) {
#if DEBUG_LOG
                            log = $"{log}\n-{characterHomeSettlement.name} doesn't have a mine or a lumberyard. Set chance to {chance}.";
#endif
                            chance = 50;
                            if (characterHomeSettlement.occupiedVillageSpot.HasUnusedMiningSpots()) {
#if DEBUG_LOG
                                log = $"{log}\n-{characterHomeSettlement.name} has an unused mining spot.";
#endif
                                //build mine
                                determinedStructureToUse = STRUCTURE_TYPE.MINE;      
                            } else {
#if DEBUG_LOG
                                log = $"{log}\n-{characterHomeSettlement.name} doesn't have an unused mining spot.";
#endif
                                //build lumberyard
                                determinedStructureToUse = STRUCTURE_TYPE.LUMBERYARD;  
                            }
                        } else if (!characterHomeSettlement.HasStructure(STRUCTURE_TYPE.WORKSHOP) && !characterHomeSettlement.HasBlueprintOnTileForStructure(STRUCTURE_TYPE.WORKSHOP)) {
                            //build workshop
                            chance = 50;
                            determinedStructureToUse = STRUCTURE_TYPE.WORKSHOP;  
#if DEBUG_LOG
                            log = $"{log}\n-{characterHomeSettlement.name} doesn't have a workshop. Set chance to {chance} and will try to build {determinedStructureToUse}";
#endif
                        }

                        //chance = 100;
                        //determinedStructureToUse = STRUCTURE_TYPE.BUTCHERS_SHOP;
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
                                    if (ShouldBuildFishery(characterHomeSettlement, ableClassesOfAllResidents)) {
                                        determinedStructureToUse = STRUCTURE_TYPE.FISHERY;
                                    } else if (ShouldBuildButcher(characterHomeSettlement, ableClassesOfAllResidents)) {
                                        determinedStructureToUse = STRUCTURE_TYPE.BUTCHERS_SHOP;
                                    } else {
                                        determinedStructureToUse = STRUCTURE_TYPE.FARM;
                                    }
#if DEBUG_LOG
                                    log = $"{log}\n-Will build {determinedStructureToUse}";
#endif
                                } else if (villagerCount > resourceSupplyCapacity) {
                                    // bool hasUnusedLumberyardSpots = characterHomeSettlement.occupiedVillageSpot.HasUnusedLumberyardSpots();
                                    bool hasUnusedMiningSpots = characterHomeSettlement.occupiedVillageSpot.HasUnusedMiningSpotsThatSettlementHasNotYetConnectedTo(characterHomeSettlement);
#if DEBUG_LOG
                                    log = $"{log}\n-Villager Count exceeds resource supply capacity.";
#endif
                                    if (characterHomeSettlement.owner != null && characterHomeSettlement.owner.factionType.type == FACTION_TYPE.Elven_Kingdom && 
                                        !characterHomeSettlement.HasStructure(STRUCTURE_TYPE.LUMBERYARD) && !characterHomeSettlement.HasBlueprintOnTileForStructure(STRUCTURE_TYPE.LUMBERYARD)) {
                                        determinedStructureToUse = STRUCTURE_TYPE.LUMBERYARD;
                                    } else if (characterHomeSettlement.owner != null && characterHomeSettlement.owner.factionType.type == FACTION_TYPE.Human_Empire && hasUnusedMiningSpots &&
                                               !characterHomeSettlement.HasStructure(STRUCTURE_TYPE.MINE) && !characterHomeSettlement.HasBlueprintOnTileForStructure(STRUCTURE_TYPE.MINE)) {
                                        determinedStructureToUse = STRUCTURE_TYPE.MINE;
                                    } else if (hasUnusedMiningSpots && !characterHomeSettlement.HasBlueprintOnTileForStructure(STRUCTURE_TYPE.MINE)) {
                                        determinedStructureToUse = STRUCTURE_TYPE.MINE;
                                    } else if (!characterHomeSettlement.HasStructure(STRUCTURE_TYPE.LUMBERYARD) && !characterHomeSettlement.HasBlueprintOnTileForStructure(STRUCTURE_TYPE.LUMBERYARD)) {
                                        determinedStructureToUse = STRUCTURE_TYPE.LUMBERYARD;
                                    }
#if DEBUG_LOG
                                    log = $"{log}\n-Determined structure to build is {determinedStructureToUse.ToString()}";
#endif
                                }
                                if (determinedStructureToUse == STRUCTURE_TYPE.NONE) {
#if DEBUG_LOG
                                    log = $"{log}\n-Checking if should build skinners lodge...";
#endif
                                    if (ShouldBuildSkinnersLodge(characterHomeSettlement, ableClassesOfAllResidents)) {
                                        determinedStructureToUse = STRUCTURE_TYPE.HUNTER_LODGE;
#if DEBUG_LOG
                                        log = $"{log}\n-Checking passed. Will try to build Skinners lodge";
#endif
                                    } else {
                                        int neededWorkShopCount = Mathf.CeilToInt((float)villagerCount / 8f);
                                        int workshopCount = characterHomeSettlement.GetStructureCount(STRUCTURE_TYPE.WORKSHOP) + characterHomeSettlement.GetNumberOfBlueprintOnTileForStructure(STRUCTURE_TYPE.WORKSHOP);
#if DEBUG_LOG
                                        log = $"{log}\n-Will check if should build workshop. Needed workshops is {neededWorkShopCount.ToString()}. Current Workshop count is {workshopCount.ToString()}";
#endif
                                        if (workshopCount < neededWorkShopCount) {
                                            determinedStructureToUse = STRUCTURE_TYPE.WORKSHOP;
#if DEBUG_LOG
                                            log = $"{log}\n-Will try to build workshop";
#endif
                                        }
                                    }    
                                }

                                if (determinedStructureToUse == STRUCTURE_TYPE.NONE) {
                                    if (character.faction != null && character.faction.factionType.type == FACTION_TYPE.Demon_Cult && 
                                        !characterHomeSettlement.HasStructure(STRUCTURE_TYPE.CULT_TEMPLE) && !characterHomeSettlement.HasBlueprintOnTileForStructure(STRUCTURE_TYPE.CULT_TEMPLE)) {
#if DEBUG_LOG
                                        log = $"{log}\n-Faction is demon cult and village does not yet have a temple";
#endif                                      
                                        determinedStructureToUse = STRUCTURE_TYPE.CULT_TEMPLE;
                                    }
                                }

                                if (determinedStructureToUse == STRUCTURE_TYPE.NONE) {
#if DEBUG_LOG
                                    log = $"{log}\n-Will check if should build tavern, hospice, prison or cemetery";
#endif
                                    if (!characterHomeSettlement.HasStructure(STRUCTURE_TYPE.TAVERN) && !characterHomeSettlement.HasBlueprintOnTileForStructure(STRUCTURE_TYPE.TAVERN)) {
                                        determinedStructureToUse = STRUCTURE_TYPE.TAVERN;    
                                    } else if (!characterHomeSettlement.HasStructure(STRUCTURE_TYPE.HOSPICE) && !characterHomeSettlement.HasBlueprintOnTileForStructure(STRUCTURE_TYPE.HOSPICE)) {
                                        determinedStructureToUse = STRUCTURE_TYPE.HOSPICE;    
                                    } else if (!characterHomeSettlement.HasStructure(STRUCTURE_TYPE.PRISON) && !characterHomeSettlement.HasBlueprintOnTileForStructure(STRUCTURE_TYPE.PRISON)) {
                                        determinedStructureToUse = STRUCTURE_TYPE.PRISON;    
                                    } else if (!characterHomeSettlement.HasStructure(STRUCTURE_TYPE.CEMETERY) && !characterHomeSettlement.HasBlueprintOnTileForStructure(STRUCTURE_TYPE.CEMETERY)) {
                                        determinedStructureToUse = STRUCTURE_TYPE.CEMETERY;    
                                    }
#if DEBUG_LOG
                                    log = $"{log}\n-Will try to build {determinedStructureToUse.ToString()}";
#endif
                                }
                            }
#if DEBUG_LOG
                            log = $"{log}\n-Final determined structure to build: {determinedStructureToUse}";
#endif
                            RuinarchListPool<string>.Release(ableClassesOfAllResidents);
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
    private bool ShouldBuildFishery(NPCSettlement p_settlement, List<string> p_ableClassesOfResidents) {
        if (p_settlement.owner != null && p_settlement.owner.factionType.IsActionConsideredACrime(CRIME_TYPE.Animal_Killing)) {
            //Animal Killing is considered a crime.
            return false;
        }
        if (!p_ableClassesOfResidents.Contains("Fisher")) {
            return false;
        }
        if (!p_settlement.occupiedVillageSpot.HasUnusedFishingSpot()) {
            return false;
        }
        if (!p_settlement.settlementJobTriggerComponent.HasTotalResource(RESOURCE.WOOD, STRUCTURE_TYPE.FISHERY.GetResourceBuildCost())) {
            return false;
        }
        return true;
    }
    private bool ShouldBuildButcher(NPCSettlement p_settlement, List<string> p_ableClassesOfResidents) {
        if (p_settlement.HasStructure(STRUCTURE_TYPE.BUTCHERS_SHOP)) {
            return false;
        }
        if (p_settlement.owner != null && p_settlement.owner.factionType.IsActionConsideredACrime(CRIME_TYPE.Animal_Killing)) {
            //Animal Killing is considered a crime.
            return false;
        }
        if (!p_ableClassesOfResidents.Contains("Butcher")) {
            return false;
        }
        if (!p_settlement.occupiedVillageSpot.HasAccessToButcherAnimals()) {
            return false;
        }
        if (!p_settlement.settlementJobTriggerComponent.HasTotalResource(RESOURCE.STONE, STRUCTURE_TYPE.BUTCHERS_SHOP.GetResourceBuildCost())) {
            return false;
        }
        return true;
    }
    private bool ShouldBuildSkinnersLodge(NPCSettlement p_settlement, List<string> p_ableClassesOfResidents) {
        if (p_settlement.HasStructure(STRUCTURE_TYPE.HUNTER_LODGE) && !p_settlement.HasBlueprintOnTileForStructure(STRUCTURE_TYPE.HUNTER_LODGE)) {
            return false;
        }
        if (!p_ableClassesOfResidents.Contains("Skinner")) {
            return false;
        }
        if (!p_settlement.occupiedVillageSpot.HasAccessToSkinnerAnimals()) {
            return false;
        }
        return true;
    }
    private bool HasActiveWorkStructureOfType(NPCSettlement p_settlement, STRUCTURE_TYPE p_structureType, string p_workerClass, List<string> p_ableClassesOfResidents) {
        if (p_settlement.HasStructure(p_structureType) || p_settlement.HasBlueprintOnTileForStructure(p_structureType)) {
            if (p_ableClassesOfResidents.Contains(p_workerClass)) {
                //I assume that if the settlement has the given structure and has a resident that is able to man it
                //I can consider that the given work structure is active or can be active.
                return true;
            }
        }
        return false;
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
