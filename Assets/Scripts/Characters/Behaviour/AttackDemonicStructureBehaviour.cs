using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using System.Linq;
using UtilityScripts;

public class AttackDemonicStructureBehaviour : CharacterBehaviourComponent {
    public AttackDemonicStructureBehaviour() {
        priority = 1080;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-{character.name} will attack demonic structure";
        if (character.partyComponent.hasParty) {
            Party counterattackParty = character.partyComponent.currentParty;
            if (!counterattackParty.isWaitTimeOver) {
                log += $"\n-Party is waiting";
                if (counterattackParty.waitingHexArea != null) {
                    log += $"\n-Party has waiting area";
                    if (character.gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
                        if (character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner == counterattackParty.waitingHexArea) {
                            log += $"\n-Character is in waiting area, roam";
                            character.jobComponent.TriggerRoamAroundTile(out producedJob);
                        } else {
                            log += $"\n-Character is not in waiting area, go to it";
                            LocationGridTile targetTile = counterattackParty.waitingHexArea.GetRandomTile();
                            character.jobComponent.CreateGoToJob(targetTile, out producedJob);
                        }
                    }
                } else {
                    log += $"\n-Party has no waiting area";
                }
            } else {
                log += $"\n-Party is not waiting";
                if (character.currentStructure == counterattackParty.target) {
                    log += $"\n-Character is already in target structure";
                    LocationStructure targetStructure = character.currentStructure;
                    if (targetStructure.objectsThatContributeToDamage.Count > 0) {
                        log += "\n-Has tile object in vision";
                        log += "\n-Adding tile object as hostile";
                        TileObject chosenTileObject = null;
                        IDamageable nearestDamageableObject = targetStructure.GetNearestDamageableThatContributeToHP(character.gridTileLocation);
                        if (nearestDamageableObject != null && nearestDamageableObject is TileObject tileObject) {
                            chosenTileObject = tileObject;
                        }
                        if (chosenTileObject != null) {
                            character.combatComponent.Fight(chosenTileObject, CombatManager.Hostility);
                        } else {
                            log += "\n-No preplaced tile object in vision";
                            log += "\n-Roam";
                            character.jobComponent.TriggerAttackDemonicStructure(out producedJob);
                        }
                    } else {
                        log += "\n-No tile object in vision";
                        log += "\n-Roam";
                        character.jobComponent.TriggerAttackDemonicStructure(out producedJob);
                    }
                } else {
                    log += $"\n-Character is not in target structure, go to it";
                    if (counterattackParty.target is LocationStructure targetStructure) {
                        LocationGridTile targetTile = UtilityScripts.CollectionUtilities.GetRandomElement(targetStructure.passableTiles);
                        character.jobComponent.TriggerAttackDemonicStructure(out producedJob, targetTile);
                    }
                    //List<LocationGridTile> tileChoices = character.behaviourComponent.attackDemonicStructureTarget.tiles.Where(x => character.movementComponent.HasPathToEvenIfDiffRegion(x)).ToList();
                    //LocationGridTile targetTile = CollectionUtilities.GetRandomElement(tileChoices);
                    //character.jobComponent.TriggerAttackDemonicStructure(out producedJob, targetTile);
                }
            }
        } else {
            if (character.behaviourComponent.attackDemonicStructureTarget.hasBeenDestroyed) {
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
                            producedJob = null;
                            return true;
                        } else {
                            log += $"\n-Still no target structure";
                        }
                    }
                }
                log += $"\n-No more demonic structure to be attacked, will remove this behaviour";
                character.marker.visionCollider.VoteToFilterVision();
                character.behaviourComponent.SetIsAttackingDemonicStructure(false, null);
            } else {
                if (character.gridTileLocation.collectionOwner.isPartOfParentRegionMap
                    && character.gridTileLocation.collectionOwner.partOfHextile == character.behaviourComponent.attackDemonicStructureTarget.occupiedHexTile) {
                    character.marker.visionCollider.VoteToUnFilterVision();
                    log += "\n-Already in the target demonic structure";
                    LocationStructure targetStructure = character.behaviourComponent.attackDemonicStructureTarget;
                    if (targetStructure.objectsThatContributeToDamage.Count > 0) {
                        log += "\n-Has tile object in vision";
                        log += "\n-Adding tile object as hostile";
                        TileObject chosenTileObject = null;
                        IDamageable nearestDamageableObject = targetStructure.GetNearestDamageableThatContributeToHP(character.gridTileLocation);
                        if (nearestDamageableObject != null && nearestDamageableObject is TileObject tileObject) {
                            chosenTileObject = tileObject;
                        }
                        if (chosenTileObject != null) {
                            character.combatComponent.Fight(chosenTileObject, CombatManager.Hostility);
                        } else {
                            log += "\n-No preplaced tile object in vision";
                            log += "\n-Roam";
                            character.jobComponent.TriggerAttackDemonicStructure(out producedJob);
                        }
                    } else {
                        log += "\n-No tile object in vision";
                        log += "\n-Roam";
                        character.jobComponent.TriggerAttackDemonicStructure(out producedJob);
                    }
                } else {
                    log += "\n-Is not in the target demonic structure";
                    log += "\n-Roam there";
                    List<LocationGridTile> tileChoices = character.behaviourComponent.attackDemonicStructureTarget.tiles
                        .Where(x => character.movementComponent.HasPathToEvenIfDiffRegion(x)).ToList();
                    LocationGridTile targetTile = CollectionUtilities.GetRandomElement(tileChoices);
                    character.jobComponent.TriggerAttackDemonicStructure(out producedJob, targetTile);
                }
            }
        }
        return true;
    }
}
