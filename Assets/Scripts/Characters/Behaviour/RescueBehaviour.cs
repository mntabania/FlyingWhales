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
        if (party.isActive) {
            RescuePartyQuest quest = party.currentQuest as RescuePartyQuest;
            if (party.partyState == PARTY_STATE.Working) {
#if DEBUG_LOG
                log += $"\n-Party is working";
#endif
                LocationStructure targetCurrentStructure = quest.targetCharacter.currentStructure;
                if (targetCurrentStructure != null && targetCurrentStructure.structureType.IsPlayerStructure()) {
#if DEBUG_LOG
                    log += $"\n-Target is in a demonic structure";
#endif
                    bool hasEndQuest = false;
                    quest.CultistBetrayalProcessing(ref hasEndQuest);
                    if (hasEndQuest) {
#if DEBUG_LOG
                        log += "\n-All members are cultists, end quest";
#endif
                        return true;
                    }
                    if (!targetCurrentStructure.hasBeenDestroyed && targetCurrentStructure.objectsThatContributeToDamage.Count > 0) {
#if DEBUG_LOG
                        log += "\n-Has tile object that contribute damage";
                        log += "\n-Adding tile object as hostile";
#endif
                        TileObject chosenTileObject = null;
                        IDamageable nearestDamageableObject = targetCurrentStructure.GetNearestDamageableThatContributeToHP(character.gridTileLocation);
                        if (nearestDamageableObject != null && nearestDamageableObject is TileObject tileObject) {
                            chosenTileObject = tileObject;
                        }
                        if (chosenTileObject != null) {
                            character.combatComponent.Fight(chosenTileObject, CombatManager.Clear_Demonic_Intrusion);
                            return true;
                        }
                    }
                }

                if (quest.targetCharacter.isBeingSeized) {
#if DEBUG_LOG
                    log += $"\n-Target is being seized";
#endif
                    LocationGridTile prevTile = quest.targetCharacter.marker.previousGridTile;
                    if (character.gridTileLocation != prevTile) {
#if DEBUG_LOG
                        log += $"\n-Character is not in previous grid tile of target, will go to it";
#endif
                        hasJob = character.jobComponent.CreateGoToSpecificTileJob(prevTile, out producedJob);
                        if (hasJob) {
                            return true;
                        }
                    } else {
#if DEBUG_LOG
                        log += $"\n-Character is already in previous grid tile of target, will end quest";
#endif
                        quest.SetIsSuccessful(true);
                        quest.EndQuest("Target is nowhere to be found");
                        return true;
                    }
                }

                if (character.hasMarker && quest.targetCharacter.hasMarker && character.gridTileLocation != null && quest.targetCharacter.gridTileLocation != null) {
                    if (character.marker.IsPOIInVision(quest.targetCharacter)) {
                        if (quest.targetCharacter.isDead) {
#if DEBUG_LOG
                            log += $"\n-Target is dead";
#endif
                            quest.SetIsSuccessful(true);
                            quest.EndQuest("Target is dead");
                            return true;
                        } else {
                            if (quest.targetCharacter.traitContainer.HasTrait("Restrained", "Unconscious", "Frozen", "Ensnared", "Enslaved")) {
#if DEBUG_LOG
                                log += $"\n-Target is incapacitated, release";
#endif
                                hasJob = character.jobComponent.TriggerReleaseJob(quest.targetCharacter, out producedJob);
                                if (hasJob) {
                                    quest.SetIsReleasing(true);
                                    return true;
                                }
                                //return hasJob;
                            } else {
#if DEBUG_LOG
                                log += $"\n-Target is not incapacitated";
#endif
                                quest.SetIsSuccessful(true);
                                quest.EndQuest("Target is safe");
                                //if target is paralyzed carry back home
                                if (quest.targetCharacter.traitContainer.HasTrait("Paralyzed")) {
                                    if (!quest.targetCharacter.IsPOICurrentlyTargetedByAPerformingAction(JOB_TYPE.RESCUE_MOVE_CHARACTER)) {
                                        //Do not set this as a party job
                                        character.jobComponent.TryTriggerRescueMoveCharacter(quest.targetCharacter, out producedJob, false);
                                    }
                                }
                                return true;
                            }
                        }
                    } else {
#if DEBUG_LOG
                        log += $"\n-Target not in vision, go to it";
#endif
                        hasJob = character.jobComponent.CreateGoToJob(quest.targetCharacter, out producedJob);
                    }
                } else {
#if DEBUG_LOG
                    log += $"\n-No markers/tile locations, end quest";
#endif
                    quest.SetIsSuccessful(true);
                    quest.EndQuest("Target is nowhere to be found");
                    return true;
                }


//                if (party.targetDestination.IsAtTargetDestination(character)) {
//#if DEBUG_LOG
//                    log += $"\n-Character is at target destination, do work";
//#endif
//                    if (character.hasMarker) {
//                        if (party.targetDestination.IsAtTargetDestination(quest.targetCharacter)) {
//                            if (quest.targetCharacter.isDead) {
//#if DEBUG_LOG
//                                log += $"\n-Target is dead";
//#endif
//                                quest.EndQuest("Target is dead");
//                                return true;
//                            } else {
//                                if (quest.targetCharacter.traitContainer.HasTrait("Restrained", "Unconscious", "Frozen", "Ensnared", "Enslaved")) {
//#if DEBUG_LOG
//                                    log += $"\n-Target is incapacitated, release";
//#endif
//                                    hasJob = character.jobComponent.TriggerReleaseJob(quest.targetCharacter, out producedJob);
//                                    if (hasJob) {
//                                        quest.SetIsReleasing(true);
//                                        return true;
//                                    }
//                                    //return hasJob;
//                                } else {
//#if DEBUG_LOG
//                                    log += $"\n-Target is not incapacitated";
//#endif
//                                    quest.EndQuest("Target is safe");
//                                    //if target is paralyzed carry back home
//                                    if (quest.targetCharacter.traitContainer.HasTrait("Paralyzed")) {
//                                        if (!quest.targetCharacter.IsPOICurrentlyTargetedByAPerformingAction(JOB_TYPE.MOVE_CHARACTER)) {
//                                            //Do not set this as a party job
//                                            character.jobComponent.TryTriggerMoveCharacter(quest.targetCharacter, out producedJob, false);
//                                        }
//                                    }
//                                    return true;
//                                }
//                            }
//                        } else {
//                            if (quest.targetCharacter.gridTileLocation != null && !quest.targetCharacter.isBeingSeized) {
//#if DEBUG_LOG
//                                log += $"\n-Target is in a different location";
//#endif
//                                //Target is still in the world, change destination
//                                party.SetTargetDestination(quest.GetTargetDestination());
//                                return true;
//                            } else {
//                                quest.EndQuest("Target is nowhere to be found");
//                                return true;
//                            }
//                        }
//                    } else {
//                        quest.EndQuest("Target is nowhere to be found");
//                        return true;
//                    }

//#if DEBUG_LOG
//                    log += $"\n-Roam around";
//#endif
//                    hasJob = RoamAroundStructureOrHex(character, party.currentQuest.target, out producedJob);
//                } else {
//                    LocationGridTile tile = party.targetDestination.GetRandomPassableTile();
//                    hasJob = character.jobComponent.CreatePartyGoToJob(tile, out producedJob);
//                }
            }
        }
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
