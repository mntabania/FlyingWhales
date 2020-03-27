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
            character.behaviourComponent.SetIsAttackingDemonicStructure(false, null);
        } else {
            if (character.gridTileLocation.structure == character.behaviourComponent.attackDemonicStructureTarget) {
                log += "\n-Already in the target demonic structure";
                LocationStructure targetStructure = character.behaviourComponent.attackDemonicStructureTarget;
                if (targetStructure.pointsOfInterest.Count > 0) {
                    log += "\n-Has tile object in vision";
                    log += "\n-Adding tile object as hostile";
                    TileObject chosenTileObject = null;
                    for (int i = 0; i < targetStructure.pointsOfInterest.Count; i++) {
                        IPointOfInterest poi = targetStructure.pointsOfInterest.ElementAt(i);
                        if(poi is TileObject tileObject) {
                            if (tileObject.isPreplaced) {
                                chosenTileObject = tileObject;
                                break;
                            }
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
