using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using System.Linq;
using UtilityScripts;

public class AttackDemonicStructureBehaviour : CharacterBehaviourComponent {
    public AttackDemonicStructureBehaviour() {
        priority = 200;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
#if DEBUG_LOG
        log += $"\n-{character.name} will attack demonic structure";
#endif
        if (character.partyComponent.hasParty) {
            Party party = character.partyComponent.currentParty;
            if (party.isActive) {
                PartyQuest quest = party.currentQuest;
                LocationStructure targetStructure = null;
                if(quest.target != null) {
                    targetStructure = party.currentQuest.target as LocationStructure;
                }
                if (targetStructure == null || targetStructure.hasBeenDestroyed || targetStructure.objectsThatContributeToDamage.Count <= 0) {
                    quest.SetIsSuccessful(true);
                    if (party.targetDestination != party.partySettlement) {
                        party.GoBackHomeAndEndQuest();
                    } else {
                        quest.EndQuest("Finished quest");
                    }
                    return true;
                }
                if (party.partyState == PARTY_STATE.Working) {
#if DEBUG_LOG
                    log += $"\n-Party is working";
#endif
                    //The checking that the character must be on the target structure first before attacking is removed because sometimes the structure is in a closed space, and if the character cannot dig, he can't attack forever because he cannot go to the structure first
                    //That is why we bypassed the checking, we immediately added the structure objects to the hostile list

                    //NOTE: Removed checking for current region because of the new party system in which the Working state must be when party is at target destination
                    //if (party.targetDestination.region == character.currentRegion) {
                    //Checking for region is added since if the target structure is in the diff inner map, there will be no path to go there

                    //NEW NOTE: With the demonic structure changes, wherein the whole structure is only 1 sprite, we must no longer check is the character is at target destination
                    //before attacking, attack it immediately
                    if (targetStructure.objectsThatContributeToDamage.Count > 0 && !targetStructure.hasBeenDestroyed) {
#if DEBUG_LOG
                        log += "\n-Has tile object that contribute damage";
                        log += "\n-Adding tile object as hostile";
#endif
                        TileObject chosenTileObject = null;
                        IDamageable nearestDamageableObject = targetStructure.GetNearestDamageableThatContributeToHP(character.gridTileLocation);
                        if (nearestDamageableObject != null && nearestDamageableObject is TileObject tileObject) {
                            chosenTileObject = tileObject;
                        }
                        if (chosenTileObject != null) {
                            character.combatComponent.Fight(chosenTileObject, CombatManager.Clear_Demonic_Intrusion);
                            return true;
                        } else {
#if DEBUG_LOG
                            log += "\n-No tile object that contribute damage/target structure is destroyed, disband party";
#endif
                            party.GoBackHomeAndEndQuest();
                            return true;
                        }
                    } else {
#if DEBUG_LOG
                        log += "\n-No tile object that contribute damage/target structure is destroyed, disband party";
#endif
                        party.GoBackHomeAndEndQuest();
                        return true;
                    }
                }
            }
        } else {
            LocationStructure targetStructure = character.behaviourComponent.attackDemonicStructureTarget;
            if (targetStructure == null || targetStructure.hasBeenDestroyed) {
#if DEBUG_LOG
                log += $"\n-Demonic structure target is already destroyed";
#endif
                //if (character is Summon summon) {
                //    if (summon.summonType == SUMMON_TYPE.Magical_Angel || summon.summonType == SUMMON_TYPE.Warrior_Angel) {
                //        log += $"\n-Character is angel, will check if there is more demonic structure to be attacked";
                //        //if (CharacterManager.Instance.currentDemonicStructureTargetOfAngels == null
                //        //    || CharacterManager.Instance.currentDemonicStructureTargetOfAngels == character.behaviourComponent.attackDemonicStructureTarget) {
                //        //    log += $"\n-No current structure target of angels, will try to set one";
                //        //    CharacterManager.Instance.SetNewCurrentDemonicStructureTargetOfAngels();
                //        //}
                //        if (CharacterManager.Instance.currentDemonicStructureTargetOfAngels != null) {
                //            log += $"\n-New target demonic structure is set: " + CharacterManager.Instance.currentDemonicStructureTargetOfAngels.structureType.ToString();
                //            character.behaviourComponent.SetDemonicStructureTarget(CharacterManager.Instance.currentDemonicStructureTargetOfAngels);
                //            return true;
                //        } else {
                //            log += $"\n-Still no target structure";
                //        }
                //    }
                //}
#if DEBUG_LOG
                log += $"\n-No more demonic structure to be attacked, will remove this behaviour";
#endif
                //character.marker.visionCollider.VoteToFilterVision();
                character.behaviourComponent.SetIsAttackingDemonicStructure(false, null);
                return true;
            } else {
                //The checking that the character must be on the target structure first before attacking is removed because sometimes the structure is in a closed space, and if the character cannot dig, he can't attack forever because he cannot go to the structure first
                //That is why we bypassed the checking, we immediately added the structure objects to the hostile list
                if (targetStructure.objectsThatContributeToDamage.Count > 0 && !targetStructure.hasBeenDestroyed) {
#if DEBUG_LOG
                    log += "\n-Has tile object that contribute damage";
                    log += "\n-Adding tile object as hostile";
#endif
                    TileObject chosenTileObject = null;
                    IDamageable nearestDamageableObject = targetStructure.GetNearestDamageableThatContributeToHP(character.gridTileLocation);
                    if (nearestDamageableObject != null && nearestDamageableObject is TileObject tileObject) {
                        chosenTileObject = tileObject;
                    }
                    if (chosenTileObject != null) {
                        character.combatComponent.Fight(chosenTileObject, CombatManager.Clear_Demonic_Intrusion);
                        return true;
                    } else {
#if DEBUG_LOG
                        log += "\n-No tile object that contribute damage/target structure is destroyed, disband party";
#endif
                        character.behaviourComponent.SetIsAttackingDemonicStructure(false, null);
                        return true;
                    }
                } else {
#if DEBUG_LOG
                    log += "\n-No tile object that contribute damage/target structure is destroyed, disband party";
#endif
                    character.behaviourComponent.SetIsAttackingDemonicStructure(false, null);
                    return true;
                }
                //LocationStructure targetStructure = character.behaviourComponent.attackDemonicStructureTarget;
                //if (targetStructure.region == character.currentRegion) {
                //    //Checking for region is added since if the target structure is in the diff inner map, there will be no path to go there
                    
                //} else {
                //    LocationGridTile targetTile = CollectionUtilities.GetRandomElement(targetStructure.occupiedArea.gridTileComponent.gridTiles.Where(x => character.movementComponent.HasPathToEvenIfDiffRegion(x)));
                //    return character.jobComponent.TriggerAttackDemonicStructure(out producedJob, targetTile);
                //}

            }
        }
        return false;
    }
}
