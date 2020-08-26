using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using System.Linq;
using UtilityScripts;

public class AttackDemonicStructureBehaviour : CharacterBehaviourComponent {
    public AttackDemonicStructureBehaviour() {
        priority = 450;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-{character.name} will attack demonic structure";
        if (character.partyComponent.hasParty) {
            Party counterattackParty = character.partyComponent.currentParty;
            if (!counterattackParty.isWaitTimeOver) {
                log += $"\n-Party is waiting";
                if (character.homeSettlement != null) {
                    log += $"\n-Character has home settlement";
                    if (character.homeSettlement.locationType == LOCATION_TYPE.DUNGEON) {
                        log += $"\n-Character home settlement is a special structure";
                        bool hasJob = character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                        if(producedJob != null) {
                            producedJob.SetIsThisAPartyJob(true);
                        }
                        return hasJob;
                    } else {
                        log += $"\n-Character home settlement is a village";
                        LocationStructure targetStructure = null;
                        if (character.currentStructure.structureType == STRUCTURE_TYPE.TAVERN) {
                            targetStructure = character.currentStructure;
                        } else {
                            targetStructure = character.homeSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.TAVERN);
                        }
                        if (targetStructure == null) {
                            if (character.currentStructure.structureType == STRUCTURE_TYPE.CITY_CENTER) {
                                targetStructure = character.currentStructure;
                            } else {
                                targetStructure = character.homeSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
                            }
                        }

                        if (targetStructure != null) {
                            log += $"\n-Character will roam around " + targetStructure.name;
                            LocationGridTile targetTile = null;
                            if (character.currentStructure != targetStructure) {
                                targetTile = UtilityScripts.CollectionUtilities.GetRandomElement(targetStructure.passableTiles);
                            }
                            bool hasJob = character.jobComponent.TriggerRoamAroundStructure(out producedJob, targetTile);
                            if (producedJob != null) {
                                producedJob.SetIsThisAPartyJob(true);
                            }
                            return hasJob;
                        }
                    }
                }
            } else {
                if (counterattackParty.target is LocationStructure targetStructure) {
                    //The checking that the character must be on the target structure first before attacking is removed because sometimes the structure is in a closed space, and if the character cannot dig, he can't attack forever because he cannot go to the structure first
                    //That is why we bypassed the checking, we immediately added the structure objects to the hostile list
                    if(targetStructure.location == character.currentRegion) {
                        //Checking for region is added since if the target structure is in the diff inner map, there will be no path to go there

                        //LocationStructure targetStructure = character.currentStructure;
                        if (targetStructure.objectsThatContributeToDamage.Count > 0 && !targetStructure.hasBeenDestroyed) {
                            log += "\n-Has tile object that contribute damage";
                            log += "\n-Adding tile object as hostile";
                            TileObject chosenTileObject = null;
                            IDamageable nearestDamageableObject = targetStructure.GetNearestDamageableThatContributeToHP(character.gridTileLocation);
                            if (nearestDamageableObject != null && nearestDamageableObject is TileObject tileObject) {
                                chosenTileObject = tileObject;
                            }
                            if (chosenTileObject != null) {
                                character.combatComponent.Fight(chosenTileObject, CombatManager.Hostility);
                                return true;
                            } else {
                                log += "\n-No tile object that contribute damage/target structure is destroyed, disband party";
                                counterattackParty.DisbandParty();
                                return true;
                            }
                        } else {
                            log += "\n-No tile object that contribute damage/target structure is destroyed, disband party";
                            counterattackParty.DisbandParty();
                            return true;
                        }
                    } else {
                        LocationGridTile targetTile = targetStructure.occupiedHexTile.hexTileOwner.GetRandomTileThatMeetCriteria(x => character.movementComponent.HasPathToEvenIfDiffRegion(x));
                        if(targetTile != null) {
                            bool hasJob = character.jobComponent.TriggerAttackDemonicStructure(out producedJob, targetTile);
                            if (producedJob != null) {
                                producedJob.SetIsThisAPartyJob(true);
                            }
                            return hasJob;
                        }
                    }
                }

                //log += $"\n-Party is not waiting";
                //if (character.currentStructure == counterattackParty.target) {
                //    log += $"\n-Character is already in target structure";
                    
                //} else {
                //    log += $"\n-Character is not in target structure, go to it";
                //    if (counterattackParty.target is LocationStructure targetStructure) {
                //        LocationGridTile targetTile = UtilityScripts.CollectionUtilities.GetRandomElement(targetStructure.passableTiles);
                //        character.jobComponent.TriggerAttackDemonicStructure(out producedJob, targetTile);
                //    }
                //    //List<LocationGridTile> tileChoices = character.behaviourComponent.attackDemonicStructureTarget.tiles.Where(x => character.movementComponent.HasPathToEvenIfDiffRegion(x)).ToList();
                //    //LocationGridTile targetTile = CollectionUtilities.GetRandomElement(tileChoices);
                //    //character.jobComponent.TriggerAttackDemonicStructure(out producedJob, targetTile);
                //}
            }
        } else {
            if (character.behaviourComponent.attackDemonicStructureTarget.hasBeenDestroyed || character.behaviourComponent.attackDemonicStructureTarget == null) {
                log += $"\n-Demonic structure target is already destroyed";
                if (character is Summon summon) {
                    if (summon.summonType == SUMMON_TYPE.Magical_Angel || summon.summonType == SUMMON_TYPE.Warrior_Angel) {
                        log += $"\n-Character is angel, will check if there is more demonic structure to be attacked";
                        if (CharacterManager.Instance.currentDemonicStructureTargetOfAngels == null
                            || CharacterManager.Instance.currentDemonicStructureTargetOfAngels == character.behaviourComponent.attackDemonicStructureTarget) {
                            log += $"\n-No current structure target of angels, will try to set one";
                            CharacterManager.Instance.SetNewCurrentDemonicStructureTargetOfAngels();
                        }
                        if (CharacterManager.Instance.currentDemonicStructureTargetOfAngels != null) {
                            log += $"\n-New target demonic structure is set: " + CharacterManager.Instance.currentDemonicStructureTargetOfAngels.structureType.ToString();
                            character.behaviourComponent.SetDemonicStructureTarget(CharacterManager.Instance.currentDemonicStructureTargetOfAngels);
                            return true;
                        } else {
                            log += $"\n-Still no target structure";
                        }
                    }
                }
                log += $"\n-No more demonic structure to be attacked, will remove this behaviour";
                character.marker.visionCollider.VoteToFilterVision();
                character.behaviourComponent.SetIsAttackingDemonicStructure(false, null);
                return true;
            } else {
                //The checking that the character must be on the target structure first before attacking is removed because sometimes the structure is in a closed space, and if the character cannot dig, he can't attack forever because he cannot go to the structure first
                //That is why we bypassed the checking, we immediately added the structure objects to the hostile list

                LocationStructure targetStructure = character.behaviourComponent.attackDemonicStructureTarget;
                if (targetStructure.location == character.currentRegion) {
                    //Checking for region is added since if the target structure is in the diff inner map, there will be no path to go there
                    if (targetStructure.objectsThatContributeToDamage.Count > 0 && !targetStructure.hasBeenDestroyed) {
                        log += "\n-Has tile object that contribute damage";
                        log += "\n-Adding tile object as hostile";
                        TileObject chosenTileObject = null;
                        IDamageable nearestDamageableObject = targetStructure.GetNearestDamageableThatContributeToHP(character.gridTileLocation);
                        if (nearestDamageableObject != null && nearestDamageableObject is TileObject tileObject) {
                            chosenTileObject = tileObject;
                        }
                        if (chosenTileObject != null) {
                            character.combatComponent.Fight(chosenTileObject, CombatManager.Hostility);
                            return true;
                        } else {
                            log += "\n-No tile object that contribute damage/target structure is destroyed, disband party";
                            character.behaviourComponent.SetDemonicStructureTarget(null);
                            return true;
                        }
                    } else {
                        log += "\n-No tile object that contribute damage/target structure is destroyed, disband party";
                        character.behaviourComponent.SetDemonicStructureTarget(null);
                        return true;
                    }
                } else {
                    LocationGridTile targetTile = CollectionUtilities.GetRandomElement(targetStructure.occupiedHexTile.hexTileOwner.locationGridTiles.Where(x => character.movementComponent.HasPathToEvenIfDiffRegion(x)));
                    return character.jobComponent.TriggerAttackDemonicStructure(out producedJob, targetTile);
                }


                //if (character.gridTileLocation.collectionOwner.isPartOfParentRegionMap
                //    && character.gridTileLocation.collectionOwner.partOfHextile == character.behaviourComponent.attackDemonicStructureTarget.occupiedHexTile) {
                //    character.marker.visionCollider.VoteToUnFilterVision();
                //    log += "\n-Already in the target demonic structure";

                //} else {
                //    log += "\n-Is not in the target demonic structure";
                //    log += "\n-Roam there";
                //    LocationGridTile targetTile = CollectionUtilities.GetRandomElement(character.behaviourComponent.attackDemonicStructureTarget.passableTiles);
                //    character.jobComponent.TriggerAttackDemonicStructure(out producedJob, targetTile);
                //}
            }
        }
        return false;
    }
}
