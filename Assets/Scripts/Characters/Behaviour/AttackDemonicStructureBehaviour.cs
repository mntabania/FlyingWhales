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
        if (character.behaviourComponent.attackDemonicStructureTarget.hasBeenDestroyed) {
            log += $"\n-Demonic structure target is already destroyed";
            if (character is Summon summon) {
                if(summon.summonType == SUMMON_TYPE.Magical_Angel || summon.summonType == SUMMON_TYPE.Warrior_Angel) {
                    log += $"\n-Character is angel, will check if there is more demonic structure to be attacked";
                    if(CharacterManager.Instance.currentDemonicStructureTargetOfAngels == null 
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
                    if(nearestDamageableObject != null && nearestDamageableObject is TileObject tileObject) {
                        chosenTileObject = tileObject;
                    }
                    //for (int i = 0; i < targetStructure.objectsThatContributeToDamage.Count; i++) {
                    //    IDamageable damageable = targetStructure.objectsThatContributeToDamage.ElementAt(i);
                    //    if (damageable is IPointOfInterest poi) {
                    //        if(poi is TileObject tileObject) {
                    //            if (tileObject.gridTileLocation != null && (tileObject.tileObjectType == TILE_OBJECT_TYPE.BLOCK_WALL ||
                    //                    PathfindingManager.Instance.HasPath(tileObject.gridTileLocation, character.gridTileLocation))) {
                    //                chosenTileObject = tileObject;
                    //                break;
                    //            }
                    //        }    
                    //    }
                    //}
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
        return true;
    }
}
