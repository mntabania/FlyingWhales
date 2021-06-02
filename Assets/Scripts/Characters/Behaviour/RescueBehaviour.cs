using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class RescueBehaviour : CharacterBehaviourComponent {
    public RescueBehaviour() {
        priority = 200;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        bool hasJob = false;
#if DEBUG_LOG
        log += $"\n-Character is rescuing";
#endif
        Party party = character.partyComponent.currentParty;
        if (party.isActive && party.partyState == PARTY_STATE.Working) {
#if DEBUG_LOG
            log += $"\n-Party is working";
#endif
            if (party.targetDestination.IsAtTargetDestination(character)) {
#if DEBUG_LOG
                log += $"\n-Character is at target destination, do work";
#endif
                RescuePartyQuest quest = party.currentQuest as RescuePartyQuest;
                if (character.hasMarker && character.marker.IsPOIInVision(quest.targetCharacter)) {
                    if (quest.targetCharacter.traitContainer.HasTrait("Restrained", "Unconscious", "Frozen", "Ensnared", "Enslaved")) {
                        hasJob = character.jobComponent.TriggerReleaseJob(quest.targetCharacter, out producedJob);
                        if (hasJob) {
                            quest.SetIsReleasing(true);
                        }
                        return hasJob;
                    } else {
                        quest.EndQuest("Saw target is safe");
                        //if target is paralyzed carry back home
                        if (!quest.targetCharacter.IsPOICurrentlyTargetedByAPerformingAction(JOB_TYPE.MOVE_CHARACTER)) {
                            //Do not set this as a party job
                            character.jobComponent.TryTriggerMoveCharacter(quest.targetCharacter, false, out producedJob);
                        }
                        return true;
                    }
                }

                Character memberInCombat = party.GetMemberInCombatExcept(character);
                if (memberInCombat != null && party.targetDestination.IsAtTargetDestination(memberInCombat)) {
#if DEBUG_LOG
                    log += $"\n-{memberInCombat.name} is in combat, will try to combat also";
#endif
                    bool hasFought = false;
                    CombatState combatState = memberInCombat.stateComponent.currentState as CombatState;
                    if (combatState.currentClosestHostile != null) {
                        CombatData combatData = memberInCombat.combatComponent.GetCombatData(combatState.currentClosestHostile);
                        character.combatComponent.Fight(combatState.currentClosestHostile, combatData.reasonForCombat, combatData.connectedAction, combatData.isLethal);
                        hasFought = true;
                    }
                    if (hasFought) {
                        producedJob = null;
                        return true;
                    }
#if DEBUG_LOG
                    log += $"\n-Roam around";
#endif
                    hasJob = RoamAroundStructureOrHex(character, party.currentQuest.target, out producedJob);
                    //character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                } else {
#if DEBUG_LOG
                    log += $"\n-Roam around";
#endif
                    //character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                    hasJob = RoamAroundStructureOrHex(character, party.currentQuest.target, out producedJob);
                }
            }
            //else {
            //    LocationGridTile tile = party.targetDestination.GetRandomPassableTile();
            //    hasJob = character.jobComponent.CreatePartyGoToJob(tile, out producedJob);
            //}
        }
        //if (!party.isWaitTimeOver) {
        //    log += $"\n-Party is waiting";
        //    if (character.homeSettlement != null) {
        //        log += $"\n-Character has home settlement";
        //        if (character.homeSettlement.locationType == LOCATION_TYPE.DUNGEON) {
        //            log += $"\n-Character home settlement is a special structure";
        //            character.jobComponent.TriggerRoamAroundStructure(out producedJob);
        //        } else {
        //            log += $"\n-Character home settlement is a village";
        //            LocationStructure targetStructure = null;
        //            if (character.currentStructure.structureType == STRUCTURE_TYPE.TAVERN) {
        //                targetStructure = character.currentStructure;
        //            } else {
        //                targetStructure = character.homeSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.TAVERN);
        //            }
        //            if (targetStructure == null) {
        //                if (character.currentStructure.structureType == STRUCTURE_TYPE.CITY_CENTER) {
        //                    targetStructure = character.currentStructure;
        //                } else {
        //                    targetStructure = character.homeSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
        //                }
        //            }

        //            if (targetStructure != null) {
        //                log += $"\n-Character will roam around " + targetStructure.name;
        //                LocationGridTile targetTile = null;
        //                if (character.currentStructure != targetStructure) {
        //                    targetTile = UtilityScripts.CollectionUtilities.GetRandomElement(targetStructure.passableTiles);
        //                }
        //                character.jobComponent.TriggerRoamAroundStructure(out producedJob, targetTile);
        //            }
        //        }
        //    }
        //} else {
        //    log += $"\n-Party is not waiting";
        //    if(character.currentStructure == party.target.currentStructure) {
        //        Character memberInCombat = party.GetMemberInCombatExcept(character);
        //        if (memberInCombat != null && memberInCombat.currentStructure == party.target.currentStructure) {
        //            log += $"\n-{memberInCombat.name} is in combat, will try to combat also";
        //            bool hasFought = false;
        //            CombatState combatState = memberInCombat.stateComponent.currentState as CombatState;
        //            if (combatState.currentClosestHostile != null) {
        //                CombatData combatData = memberInCombat.combatComponent.GetCombatData(combatState.currentClosestHostile);
        //                character.combatComponent.Fight(combatState.currentClosestHostile, combatData.reasonForCombat, combatData.connectedAction, combatData.isLethal);
        //                hasFought = true;
        //            }
        //            //else {
        //            //    if (memberInCombat.combatComponent.avoidInRange.Count > 0) {
        //            //        for (int i = 0; i < memberInCombat.combatComponent.avoidInRange.Count; i++) {
        //            //            if (memberInCombat.combatComponent.avoidInRange[i] is Character targetCharacter) {
        //            //                character.combatComponent.Fight(targetCharacter, CombatManager.Hostility);
        //            //                hasFought = true;
        //            //            }
        //            //        }
        //            //    }
        //            //}
        //            if (hasFought) {
        //                producedJob = null;
        //                return true;
        //            }
        //            log += $"\n-Roam around";
        //            RoamAroundStructureOrHex(character, party.target, out producedJob);
        //            //character.jobComponent.TriggerRoamAroundStructure(out producedJob);
        //        } else {
        //            log += $"\n-Roam around";
        //            //character.jobComponent.TriggerRoamAroundStructure(out producedJob);
        //            RoamAroundStructureOrHex(character, party.target, out producedJob);
        //        }
        //    } else {
        //        log += $"\n-Character is not in target structure, go to it";
        //        LocationGridTile targetTile = UtilityScripts.CollectionUtilities.GetRandomElement(party.target.currentStructure.passableTiles);
        //        character.jobComponent.CreatePartyGoToJob(targetTile, out producedJob);
        //    }
        //}
        if (producedJob != null) {
            producedJob.SetIsThisAPartyJob(true);
        }
        return hasJob;
    }

    private bool RoamAroundStructureOrHex(Character actor, IPartyQuestTarget target, out JobQueueItem producedJob) {
        if(target != null && target.currentStructure != null && target.currentStructure.structureType == STRUCTURE_TYPE.WILDERNESS) {
            if(target is Character targetCharacter && targetCharacter.gridTileLocation != null) {
                Area targetArea = targetCharacter.areaLocation;
                //Job type is Roam Around Structure because the Roam Around Tile job priority is less than the Rescue Behaviour
                return actor.jobComponent.TriggerRoamAroundTile(JOB_TYPE.ROAM_AROUND_STRUCTURE, out producedJob, targetArea.gridTileComponent.GetRandomTile());
            }
        }
        //When roaming around structure or hex relative to the target and the target is not in a tile that we expect him to be, just roam aroung current structure to avoid null refs
        return actor.jobComponent.TriggerRoamAroundStructure(out producedJob);
    }
}
