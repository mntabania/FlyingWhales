using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Traits;

public class FactionLeaderBehaviour : CharacterBehaviourComponent {
    public FactionLeaderBehaviour() {
        priority = 20;
        attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.ONCE_PER_DAY };
    }

    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        log += $"\n-{character.name} is a faction leader";
        Faction faction = character.faction;
        if(faction != null && faction.factionType.HasIdeology(FACTION_IDEOLOGY.Warmonger) && !faction.HasJob(JOB_TYPE.RAID)) {
            log += $"\n-10% chance to declare raid";
            int roll = UnityEngine.Random.Range(0, 100);
            if(roll < 10) {
                if (!faction.partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Raid)) {
                    log += $"\n-Character faction is warmonger and has no raid job and has no raid party yet";
                    Faction targetFaction = faction.GetRandomAtWarFaction();
                    if (targetFaction != null) {
                        log += $"\n-Chosen target faction: " + targetFaction.name;
                        BaseSettlement targetSettlement = targetFaction.GetRandomOwnedSettlement();
                        if (targetSettlement != null) {
                            log += $"\n-Chosen target settlement: " + targetSettlement.name;
                            LocationStructure targetStructure = targetSettlement.GetRandomStructure();
                            if (targetSettlement is NPCSettlement npcSettlement && npcSettlement.cityCenter != null) {
                                targetStructure = npcSettlement.cityCenter;
                            }
                            character.interruptComponent.SetRaidTargetSettlement(targetSettlement);
                            if (character.interruptComponent.TriggerInterrupt(INTERRUPT.Declare_Raid, character)) {
                                producedJob = null;
                                return true;
                            }
                        }
                    }
                }
            }
        }
        if (character.homeSettlement != null) {
            if (character.homeSettlement.prison != null && character.faction != null) {
                LocationStructure structure = character.homeSettlement.prison;
                log += $"\n-15% chance to recruit a restrained character from different faction";
                int roll = Random.Range(0, 100);
                log += $"\n-Roll: {roll}";
                if (roll < 15) {
                    Character targetCharacter = structure.GetRandomCharacterThatCanBeRecruitedBy(character);

                    if(targetCharacter != null) {
                        log += $"\n-Chosen target: {targetCharacter.name}";
                        return character.jobComponent.TriggerRecruitJob(targetCharacter, out producedJob);
                    }
                }    
            }
            if (character.homeSettlement.settlementType != null) {
                // int existingBuildJobs = character.homeSettlement.GetNumberOfJobsWith(JOB_TYPE.BUILD_BLUEPRINT);
                List<JobQueueItem> buildJobs = RuinarchListPool<JobQueueItem>.Claim();
                character.homeSettlement.PopulateJobsOfType(buildJobs, JOB_TYPE.BUILD_BLUEPRINT);
                if (buildJobs.Count < 2) {
                    log += $"\n-Check chance to build dwelling if not yet at max.";
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
                        // chance = 0;
                        if (GameUtilities.RollChance(chance, ref log)) {
                            log += $"\n-Chance met and dwellings not yet at maximum.";
                            //place dwelling blueprint
                            StructureSetting structureToPlace = character.homeSettlement.settlementType.GetDwellingSetting(character.faction);
                            if (LandmarkManager.Instance.CanPlaceStructureBlueprint(character.homeSettlement, structureToPlace, out var targetTile, out var structurePrefabName, out var connectorToUse, out var connectorTile)) {
                                log += $"\n-Will place dwelling blueprint {structurePrefabName} at {targetTile}.";
                                RuinarchListPool<JobQueueItem>.Release(buildJobs);
                                return character.jobComponent.TriggerPlaceBlueprint(structurePrefabName, connectorToUse, structureToPlace, targetTile, connectorTile, out producedJob);    
                            }    
                        }
                    }
                    if(dwellingCount <= 0) {
                        log += $"\n-Settlement has no dwelling yet, always build dwelling first";
                        //place dwelling blueprint
                        StructureSetting structureToPlace = character.homeSettlement.settlementType.GetDwellingSetting(character.faction);
                        if (LandmarkManager.Instance.CanPlaceStructureBlueprint(character.homeSettlement, structureToPlace, out var targetTile, out var structurePrefabName, out var connectorToUse, out var connectorTile)) {
                            log += $"\n-Will place dwelling blueprint {structurePrefabName} at {targetTile}.";
                            RuinarchListPool<JobQueueItem>.Release(buildJobs);
                            return character.jobComponent.TriggerPlaceBlueprint(structurePrefabName, connectorToUse, structureToPlace, targetTile, connectorTile, out producedJob);
                        }
                    }
                    log += $"\n-Check chance to build a missing facility.";
                    int facilityCount = character.homeSettlement.GetFacilityCount();
                    int totalFacilityCount = facilityCount + GetJobsThatWillBuildFacility(buildJobs);
                    
                    if (totalFacilityCount < character.homeSettlement.settlementType.maxFacilities) {
                        int chance = 2;
                        if (facilityCount < (character.homeSettlement.settlementType.maxFacilities/2)) {
                            chance = 3;
                        }
                        if(!character.homeSettlement.HasStructure(STRUCTURE_TYPE.LUMBERYARD) && !character.homeSettlement.HasStructure(STRUCTURE_TYPE.MINE_SHACK)) {
                            chance *= 2;
                        }
                        // chance = 100;
                        if (GameUtilities.RollChance(chance, ref log)) {
                            log += $"\n-Chance to build facility met.";
                            //place random facility based on weights
                            StructureSetting targetFacility = character.homeSettlement.GetMissingFacilityToBuildBasedOnWeights();
                            log += $"\n-Will try to build facility {targetFacility.ToString()}";
                            if (targetFacility.hasValue && LandmarkManager.Instance.CanPlaceStructureBlueprint(character.homeSettlement, targetFacility, out var targetTile, out var structurePrefabName, out var connectorToUse, out var connectorTile)) {
                                log += $"\n-Will place blueprint {structurePrefabName} at {targetTile}.";
                                RuinarchListPool<JobQueueItem>.Release(buildJobs);
                                return character.jobComponent.TriggerPlaceBlueprint(structurePrefabName, connectorToUse, targetFacility, targetTile, connectorTile, out producedJob);    
                            } else {
                                log += $"\n-Could not find location to place facility {targetFacility.ToString()}";
                            }
                        }
                    }
                } else {
                    log += $"\n-Maximum build blueprint jobs reached.";
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
}
