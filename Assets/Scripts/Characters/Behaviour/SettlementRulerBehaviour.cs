using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;
using Inner_Maps.Location_Structures;

public class SettlementRulerBehaviour : CharacterBehaviourComponent {
    public SettlementRulerBehaviour() {
        priority = 22;
        attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY, BEHAVIOUR_COMPONENT_ATTRIBUTE.ONCE_PER_DAY };
    }

    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        log += $"\n-{character.name} is a settlement ruler";
        if (character.homeSettlement != null && character.homeSettlement.prison != null) {
            if(character.faction != null) {
                LocationStructure structure = character.homeSettlement.prison;
                log += $"\n-15% chance to recruit a restrained character from different faction";
                int roll = Random.Range(0, 100);
                log += $"\n-Roll: {roll}";
                if (roll < 15) {
                    Character targetCharacter = structure.GetRandomCharacterThatMeetCriteria(x => CanCharacterBeRecruited(x, character));

                    if (targetCharacter != null) {
                        log += $"\n-Chosen target: {targetCharacter.name}";
                        return character.jobComponent.TriggerRecruitJob(targetCharacter, out producedJob);
                    }
                }
            }
            if (character.homeSettlement.settlementType != null) {
                int existingBuildJobs = character.homeSettlement.GetNumberOfJobsWith(JOB_TYPE.BUILD_BLUEPRINT);
                if (existingBuildJobs < 2) {
                    log += $"\n-Check chance to build dwelling if not yet at max.";
                    int dwellingCount = character.homeSettlement.GetStructureCount(STRUCTURE_TYPE.DWELLING);
                    if (dwellingCount < character.homeSettlement.settlementType.maxDwellings) {
                        int chance = 3;
                        if (dwellingCount < (character.homeSettlement.settlementType.maxDwellings/2)) {
                            chance = 5;
                        }
                        if (character.homeSettlement.HasHomelessResident()) {
                            chance = 7;
                        }
                        if (GameUtilities.RollChance(chance, ref log)) {
                            log += $"\n-Chance met and dwellings not yet at maximum.";
                            //place dwelling blueprint
                            StructureSetting structureToPlace = character.homeSettlement.settlementType.GetDwellingSetting(character.faction);
                            if (LandmarkManager.Instance.CanPlaceStructureBlueprint(character.homeSettlement, structureToPlace, out var targetTile, out var structurePrefabName, out var connectorToUse)) {
                                log += $"\n-Will place dwelling blueprint {structurePrefabName} at {targetTile}.";
                                return character.jobComponent.TriggerPlaceBlueprint(structurePrefabName, connectorToUse, structureToPlace, targetTile, out producedJob);    
                            }    
                        }
                    }
                    log += $"\n-Check chance to build a missing facility.";
                    int facilityCount = character.homeSettlement.GetFacilityCount();
                    if (facilityCount < character.homeSettlement.settlementType.maxFacilities) {
                        int chance = 2;
                        if (facilityCount < (character.homeSettlement.settlementType.maxFacilities/2)) {
                            chance = 3;
                        }
                        if (GameUtilities.RollChance(chance, ref log)) {
                            log += $"\n-Chance to build facility met.";
                            //place random facility based on weights
                            StructureSetting targetFacility = character.homeSettlement.GetMissingFacilityToBuildBasedOnWeights();
                            if (targetFacility.hasValue && LandmarkManager.Instance.CanPlaceStructureBlueprint(character.homeSettlement, targetFacility, out var targetTile, out var structurePrefabName, out var connectorToUse)) {
                                log += $"\n-Will place blueprint {structurePrefabName} at {targetTile}.";
                                return character.jobComponent.TriggerPlaceBlueprint(structurePrefabName, connectorToUse, targetFacility, targetTile, out producedJob);    
                            }
                        }
                    }
                } else {
                    log += $"\n-Maximum build blueprint jobs reached.";
                }
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

    private bool CanCharacterBeRecruited(Character targetCharacter, Character recruiter) {
        if (!targetCharacter.traitContainer.HasTrait("Restrained")) {
            return false;
        }
        if (targetCharacter.faction == recruiter.faction) {
            return false;
        }
        if (targetCharacter.HasJobTargetingThis(JOB_TYPE.RECRUIT)) {
            return false;
        }
        if (!recruiter.faction.ideologyComponent.DoesCharacterFitCurrentIdeologies(targetCharacter)) {
            //Cannot recruit characters that does not fit faction ideologies
            return false;
        }
        if (recruiter.faction.IsCharacterBannedFromJoining(targetCharacter)) {
            //Cannot recruit banned characters
            return false;
        }
        return true;
    }
}
