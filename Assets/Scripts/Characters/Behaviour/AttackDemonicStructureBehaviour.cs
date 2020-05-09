using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using System.Linq;

public class AttackDemonicStructureBehaviour : CharacterBehaviourComponent {
    public AttackDemonicStructureBehaviour() {
        priority = 0;
    }
    public override bool TryDoBehaviour(Character character, ref string log) {
        log += $"\n-{character.name} will attack demonic structure";
        if (character.behaviourComponent.attackDemonicStructureTarget.hasBeenDestroyed) {
            log += $"\n-Demonic structure target is already destroyed";
            if (character is Summon summon) {
                if(summon.summonType == SUMMON_TYPE.Magical_Angel || summon.summonType == SUMMON_TYPE.Warrior_Angel) {
                    log += $"\n-Character is angel, will check if there is more demonic structure to be attacked";
                    if(CharacterManager.Instance.currentDemonicStructureTargetOfAngels == null) {
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
        } else {
            if (character.gridTileLocation.structure == character.behaviourComponent.attackDemonicStructureTarget) {
                character.marker.visionCollider.VoteToUnFilterVision();
                log += "\n-Already in the target demonic structure";
                LocationStructure targetStructure = character.behaviourComponent.attackDemonicStructureTarget;
                if (targetStructure.objectsThatContributeToDamage.Count > 0) {
                    log += "\n-Has tile object in vision";
                    log += "\n-Adding tile object as hostile";
                    TileObject chosenTileObject = null;
                    for (int i = 0; i < targetStructure.objectsThatContributeToDamage.Count; i++) {
                        IDamageable damageable = targetStructure.objectsThatContributeToDamage.ElementAt(i);
                        if (damageable is IPointOfInterest poi) {
                            if(poi is TileObject tileObject) {
                                if (tileObject.isPreplaced && tileObject.gridTileLocation != null 
                                    && PathfindingManager.Instance.HasPath(tileObject.gridTileLocation, character.gridTileLocation)) {
                                    chosenTileObject = tileObject;
                                    break;
                                }
                            }    
                        }
                    }
                    if (chosenTileObject != null) {
                        character.combatComponent.Fight(chosenTileObject, CombatManager.Hostility);
                    } else {
                        log += "\n-No preplaced tile object in vision";
                        log += "\n-Roam";
                        character.jobComponent.TriggerAttackDemonicStructure();
                    }
                } else {
                    log += "\n-No tile object in vision";
                    log += "\n-Roam";
                    character.jobComponent.TriggerAttackDemonicStructure();
                }
            } else {
                log += "\n-Is not in the target demonic structure";
                log += "\n-Roam there";
                LocationGridTile targetTile = character.behaviourComponent.attackDemonicStructureTarget.tiles[UnityEngine.Random.Range(0, character.behaviourComponent.attackDemonicStructureTarget.tiles.Count)];
                character.jobComponent.TriggerAttackDemonicStructure(targetTile);
            }
        }
        return true;
    }
}
