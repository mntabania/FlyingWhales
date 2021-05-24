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
        if (character.homeSettlement != null && character.homeSettlement.prison != null) {
            if(character.faction != null) {
                LocationStructure structure = character.homeSettlement.prison;
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
            if (character.homeSettlement.settlementType != null) {
                // int existingBuildJobs = character.homeSettlement.GetNumberOfJobsWith(JOB_TYPE.BUILD_BLUEPRINT);
                List<JobQueueItem> buildJobs = RuinarchListPool<JobQueueItem>.Claim();
                character.homeSettlement.PopulateJobsOfType(buildJobs, JOB_TYPE.BUILD_BLUEPRINT);
                if (buildJobs.Count < 2) {
#if DEBUG_LOG
                    log += $"\n-Check chance to build dwelling if not yet at max.";
#endif
                    int dwellingCount = character.homeSettlement.GetStructureCount(STRUCTURE_TYPE.DWELLING);
                    int totalDwellingCount = dwellingCount + GetJobsThatWillBuildDwelling(buildJobs);
                    if (totalDwellingCount < character.homeSettlement.settlementType.maxDwellings) {
                        int chance = 3;
                        if (dwellingCount < (character.homeSettlement.settlementType.maxDwellings/2)) {
                            chance = 5;
                        }
                        if (character.homeSettlement.HasHomelessResident()) {
                            chance = 7;
                        }
                        if (GameUtilities.RollChance(chance, ref log)) {
#if DEBUG_LOG
                            log += $"\n-Chance met and dwellings not yet at maximum.";
#endif
                            //place dwelling blueprint
                            StructureSetting structureToPlace = character.homeSettlement.settlementType.GetDwellingSetting(character.faction);
                            if (character.homeSettlement.owner != null) {
                                structureToPlace = character.homeSettlement.owner.factionType.ProcessStructureSetting(structureToPlace, character.homeSettlement);    
                            }
                            if (LandmarkManager.Instance.CanPlaceStructureBlueprint(character.homeSettlement, structureToPlace, out var targetTile, out var structurePrefabName, out var connectorToUse, out var connectorTile)) {
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
                    int facilityCount = character.homeSettlement.GetFacilityCount();
                    int totalFacilityCount = facilityCount + GetJobsThatWillBuildFacility(buildJobs);
                    if (totalFacilityCount < character.homeSettlement.settlementType.maxFacilities) {
                        int chance = 2;
                        if (facilityCount < (character.homeSettlement.settlementType.maxFacilities/2)) {
                            chance = 3;
                        }
                        if (GameUtilities.RollChance(chance, ref log)) {
#if DEBUG_LOG
                            log += $"\n-Chance to build facility met.";
#endif
                            //place random facility based on weights
                            StructureSetting targetFacility = character.homeSettlement.GetMissingFacilityToBuildBasedOnWeights();
                            if (character.homeSettlement.owner != null) {
                                targetFacility = character.homeSettlement.owner.factionType.ProcessStructureSetting(targetFacility, character.homeSettlement);    
                            }
#if DEBUG_LOG
                            log += $"\n-Will try to build facility {targetFacility.ToString()}";
#endif
                            if (targetFacility.hasValue && LandmarkManager.Instance.CanPlaceStructureBlueprint(character.homeSettlement, targetFacility, out var targetTile, out var structurePrefabName, out var connectorToUse, out var connectorTile)) {
#if DEBUG_LOG
                                log += $"\n-Will place blueprint {structurePrefabName} at {targetTile}.";
#endif
                                RuinarchListPool<JobQueueItem>.Release(buildJobs);
                                return character.jobComponent.TriggerPlaceBlueprint(structurePrefabName, connectorToUse, targetFacility, targetTile, connectorTile, out producedJob);    
                            } else {
#if DEBUG_LOG
                                log += $"\n-Could not find location to place facility {targetFacility.ToString()}";
#endif
                            }
                        }
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
        //log += $"\n-{character.name} will try to place blueprint";
        //if (character.isAtHomeRegion && character.homeSettlement != null && character.homeSettlement.GetNumberOfJobsWith(JOB_TYPE.BUILD_BLUEPRINT) < 2 && HasCharacterWithPlaceBlueprintJobInSettlement(character.homeSettlement) == false) {
        //    log += $"\n-{character.name} will roll for blueprint placement.";
        //    int chance = 35;
        //    int roll = Random.Range(0, 100);
        //    log += $"\n-Roll is {roll.ToString()}, chance is {chance.ToString()}";
        //    if (roll < chance) {
        //        log += $"\n-Roll successful";
        //        STRUCTURE_TYPE neededStructure = character.buildStructureComponent.GetCurrentStructureToBuild();
        //        log += $"\n-Structure Type to build is {neededStructure.ToString()}";

        //        List<GameObject> choices = InnerMapManager.Instance.GetStructurePrefabsForStructure(neededStructure);
        //        GameObject chosenStructurePrefab = CollectionUtilities.GetRandomElement(choices);
        //        log += $"\n-Structure Prefab chosen is {chosenStructurePrefab.name}";

        //        LocationStructureObject lso = chosenStructurePrefab.GetComponent<LocationStructureObject>();
        //        StructureTileObject chosenBuildingSpot;
        //        // if (character.homeRegion.innerMap.TryGetValidBuildSpotTileObjectForStructure(lso, character.homeSettlement, out chosenBuildingSpot) == false) {
        //        //     log += $"\n-Could not find spot that can house new structure. Abandoning...";
        //        //     return false;
        //        // }
        //        log += $"\n-Creating new Place Blueprint job targeting {chosenBuildingSpot.ToString()} at {chosenBuildingSpot.gridTileLocation.ToString()}";
        //        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PLACE_BLUEPRINT, INTERACTION_TYPE.PLACE_BLUEPRINT, chosenBuildingSpot, character);
        //        job.AddOtherData(INTERACTION_TYPE.PLACE_BLUEPRINT, new object[] { neededStructure });
        //        character.jobQueue.AddJobInQueue(job);

        //        return true;
        //    }
        //}
        //log += $"\n-{character.name} failed to place blueprint";
        //return false;
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
}
