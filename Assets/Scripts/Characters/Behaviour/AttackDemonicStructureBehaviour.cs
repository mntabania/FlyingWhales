using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class AttackDemonicStructureBehaviour : CharacterBehaviourComponent {
    public AttackDemonicStructureBehaviour() {
        priority = 0;
    }
    public override bool TryDoBehaviour(Character character, ref string log) {
        log += $"\n-{character.name} will attack demonic structure";
        if (character.behaviourComponent.attackDemonicStructureTarget.hasBeenDestroyed) {
            character.behaviourComponent.SetIsAttackingDemonicStructure(false, null);
        } else {
            if (character.gridTileLocation.structure == character.behaviourComponent.attackDemonicStructureTarget) {
                log += "\n-Already in the target demonic structure";
                if (character.marker.inVisionTileObjects.Count > 0) {
                    log += "\n-Has tile object in vision";
                    log += "\n-Adding tile object as hostile";
                    TileObject chosenTileObject = null;
                    for (int i = 0; i < character.marker.inVisionTileObjects.Count; i++) {
                        TileObject tileObject = character.marker.inVisionTileObjects[i];
                        if (tileObject.isPreplaced) {
                            chosenTileObject = tileObject;
                            break;
                        }
                    }
                    if (chosenTileObject != null) {
                        character.combatComponent.Fight(chosenTileObject);
                    } else {
                        log += "\n-No preplaced tile object in vision";
                        log += "\n-Roam";
                        character.jobComponent.TriggerRoamAroundTile();
                    }
                } else {
                    log += "\n-No tile object in vision";
                    log += "\n-Roam";
                    character.jobComponent.TriggerRoamAroundTile();
                }
            } else {
                log += "\n-Is not in the target demonic structure";
                log += "\n-Roam there";
                LocationGridTile targetTile = character.behaviourComponent.attackDemonicStructureTarget.tiles[UnityEngine.Random.Range(0, character.behaviourComponent.attackDemonicStructureTarget.tiles.Count)];
                character.jobComponent.TriggerRoamAroundTile(targetTile);

            }
        }
        return true;
    }
}
