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
    public override bool TryDoBehaviour(Character character, ref string log) {
        log += $"\n-{character.name} will attack demonic structure";
        if (character.behaviourComponent.attackDemonicStructureTarget.hasBeenDestroyed) {
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
                    for (int i = 0; i < targetStructure.objectsThatContributeToDamage.Count; i++) {
                        IDamageable damageable = targetStructure.objectsThatContributeToDamage.ElementAt(i);
                        if (damageable is IPointOfInterest poi) {
                            if(poi is TileObject tileObject) {
                                if (tileObject.gridTileLocation != null 
                                    && (tileObject.tileObjectType == TILE_OBJECT_TYPE.BLOCK_WALL ||
                                        PathfindingManager.Instance.HasPath(tileObject.gridTileLocation, character.gridTileLocation))) {
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
                List<LocationGridTile> tileChoices = character.behaviourComponent.attackDemonicStructureTarget.tiles
                    .Where(x => PathfindingManager.Instance.HasPathEvenDiffRegion(character.gridTileLocation, x)).ToList();
                LocationGridTile targetTile = CollectionUtilities.GetRandomElement(tileChoices);
                character.jobComponent.TriggerAttackDemonicStructure(targetTile);
            }
        }
        return true;
    }
}
