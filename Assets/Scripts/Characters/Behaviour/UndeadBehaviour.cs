using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

public class UndeadBehaviour : CharacterBehaviourComponent {
	public UndeadBehaviour() {
		priority = 9;
	}
	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
#if DEBUG_LOG
        log += $"\n-{character.name} is an undead";
#endif
        if (character.race == RACE.SKELETON) {
#if DEBUG_LOG
            log += $"\n-Character is a skeleton";
#endif
            Faction undeadFaction = FactionManager.Instance.undeadFaction;
            if(undeadFaction.leader != null && undeadFaction.leader is Character undeadFactionLeader) {
#if DEBUG_LOG
                log += $"\n-Undead faction has a character leader";
#endif
                if (undeadFactionLeader.homeStructure != null) {
                    LocationStructure lair = undeadFactionLeader.homeStructure;
#if DEBUG_LOG
                    log += $"\n-Undead faction leader has a lair";
                    log += $"\n-Character must migrate there if the lair is not yet his home";
#endif
                    if (character.homeStructure != lair) {
#if DEBUG_LOG
                        log += $"\n-Character migrated to the lair";
#endif
                        character.MigrateHomeStructureTo(lair);
                        character.ClearTerritory();
                    }
                    if (character.currentStructure == lair && undeadFactionLeader.currentStructure == lair) {
                        if (undeadFactionLeader.combatComponent.isInCombat) {
#if DEBUG_LOG
                            log += $"\n-Faction leader in combat, will try to combat also";
#endif
                            bool hasFought = false;
                            CombatState combatState = undeadFactionLeader.stateComponent.currentState as CombatState;
                            if (combatState.currentClosestHostile != null) {
                                CombatData combatData = undeadFactionLeader.combatComponent.GetCombatData(combatState.currentClosestHostile);
                                character.combatComponent.Fight(combatState.currentClosestHostile, combatData.reasonForCombat, combatData.connectedAction, combatData.isLethal);
                                hasFought = true;
                            } else {
                                if (undeadFactionLeader.combatComponent.avoidInRange.Count > 0) {
                                    for (int i = 0; i < undeadFactionLeader.combatComponent.avoidInRange.Count; i++) {
                                        if (undeadFactionLeader.combatComponent.avoidInRange[i] is Character targetCharacter) {
                                            character.combatComponent.Fight(targetCharacter, CombatManager.Hostility);
                                            hasFought = true;
                                        }
                                    }
                                }
                            }
                            if (hasFought) {
                                producedJob = null;
                                return true;
                            }
                        }
#if DEBUG_LOG
                        log += $"\n-Character and faction leader is in lair, roam";
#endif
                        character.jobComponent.TriggerRoamAroundTile(out producedJob);
                        return true;
                    } else {
                        if (!undeadFactionLeader.isBeingSeized && undeadFactionLeader.marker && undeadFactionLeader.gridTileLocation != null && !undeadFactionLeader.isDead
                            && character.gridTileLocation != null && character.movementComponent.HasPathToEvenIfDiffRegion(undeadFactionLeader.gridTileLocation)) {
                            if (character.marker.IsPOIInVision(undeadFactionLeader)) {
#if DEBUG_LOG
                                log += $"\n-Character can see faction leader";
#endif
                                if (undeadFactionLeader.combatComponent.isInCombat) {
#if DEBUG_LOG
                                    log += $"\n-Faction leader in combat, will try to combat also";
#endif
                                    CombatState combatState = undeadFactionLeader.stateComponent.currentState as CombatState;
                                    if (combatState.currentClosestHostile != null) {
                                        CombatData combatData = undeadFactionLeader.combatComponent.GetCombatData(combatState.currentClosestHostile);
                                        character.combatComponent.Fight(combatState.currentClosestHostile, combatData.reasonForCombat, combatData.connectedAction, combatData.isLethal);
                                    } else {
                                        if(undeadFactionLeader.combatComponent.avoidInRange.Count > 0) {
                                            for (int i = 0; i < undeadFactionLeader.combatComponent.avoidInRange.Count; i++) {
                                                if(undeadFactionLeader.combatComponent.avoidInRange[i] is Character targetCharacter) {
                                                    character.combatComponent.Fight(targetCharacter, CombatManager.Hostility);
                                                }
                                            }
                                        }
                                    }
                                } else {
#if DEBUG_LOG
                                    log += $"\n-Do nothing";
#endif
                                }
                                producedJob = null;
                                return true;
                            } else {
#if DEBUG_LOG
                                log += $"\n-Character cannot see faction leader, go to him";
#endif
                                if (character.jobComponent.CreateGoToJob(undeadFactionLeader)) {
                                    producedJob = null;
                                    return true;
                                }
                            }
                        } else {
                            if(character.currentStructure != lair) {
                                character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
                                return true;
                            } else {
                                character.jobComponent.TriggerRoamAroundTile(out producedJob);
                                return true;
                            }
                        }
                    }
                }
            }
        }
        producedJob = null;
        return false;
	}
}
