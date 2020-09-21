using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class ExploreBehaviour : CharacterBehaviourComponent {
    public ExploreBehaviour() {
        priority = 200;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        bool hasJob = false;
        log += $"\n-Character is exploring";
        Party party = character.partyComponent.currentParty;
        if (party.isActive && party.partyState == PARTY_STATE.Working) {
            log += $"\n-Party is working";
            if (party.targetDestination.IsAtTargetDestination(character)) {
                log += $"\n-Character is at target destination, do work";
                Character memberInCombat = party.GetMemberInCombatExcept(character);
                if (memberInCombat != null && party.targetDestination.IsAtTargetDestination(memberInCombat)) {
                    log += $"\n-{memberInCombat.name} is in combat, will try to combat also";
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
                    log += $"\n-Roam around";
                    hasJob = character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                } else {
                    log += $"\n-Roam around";
                    hasJob = character.jobComponent.TriggerRoamAroundStructure(out producedJob);
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
        //            if(character.currentStructure.structureType == STRUCTURE_TYPE.TAVERN) {
        //                targetStructure = character.currentStructure;
        //            } else {
        //                targetStructure = character.homeSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.TAVERN);
        //            }
        //            if (targetStructure == null) {
        //                if (character.currentStructure.structureType == STRUCTURE_TYPE.CITY_CENTER) {
        //                    targetStructure = character.currentStructure;
        //                } else {
        //                    targetStructure = character.homeSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
        //                }
        //            }

        //            if(targetStructure != null) {
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
        //    if(character.currentStructure == party.target) {
        //        log += $"\n-Character is already in target structure";
        //        Character memberInCombat = party.GetMemberInCombatExcept(character);
        //        if (memberInCombat != null && memberInCombat.currentStructure == party.target) {
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
        //            character.jobComponent.TriggerRoamAroundStructure(out producedJob);
        //        } else {
        //            log += $"\n-Roam around";
        //            character.jobComponent.TriggerRoamAroundStructure(out producedJob);
        //        }
        //    } else {
        //        log += $"\n-Character is not in target structure, go to it";
        //        if (party.target is LocationStructure targetStructure) {
        //            LocationGridTile targetTile = UtilityScripts.CollectionUtilities.GetRandomElement(targetStructure.passableTiles);
        //            if(targetTile != null) {
        //                character.jobComponent.CreatePartyGoToJob(targetTile, out producedJob);
        //            } else {
        //                if(party is ExplorationParty party) {
        //                    if(party.targetStructure == null || party.targetStructure.hasBeenDestroyed || party.targetStructure.tiles.Count <= 0) {
        //                        party.ProcessSettingTargetStructure();
        //                    }
        //                }
        //            }
        //            //else {
        //            //    throw new System.Exception("No passable tiles for exploring " + targetStructure.name + " of " + character.name);
        //            //}
        //        }
        //    }
        //}
        if (producedJob != null) {
            producedJob.SetIsThisAPartyJob(true);
        }
        return hasJob;
    }
}
