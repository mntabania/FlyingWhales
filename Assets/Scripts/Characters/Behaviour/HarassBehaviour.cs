using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class HarassBehaviour : CharacterBehaviourComponent {
    public HarassBehaviour() {
        priority = 10;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-{character.name} will harass";
        if (character.gridTileLocation.collectionOwner.partOfHextile != null
            && character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner 
            && character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.settlementOnTile == character.behaviourComponent.assignedTargetSettlement) {
            log += "\n-Already in the target npcSettlement";
            if(character.marker.inVisionTileObjects.Count > 0) {
                log += "\n-Has tile object in vision";
                log += "\n-Adding tile object as hostile";
                character.combatComponent.Fight(character.marker.inVisionTileObjects[0], CombatManager.Hostility);
            } else {
                log += "\n-No tile object in vision";
                log += "\n-Roam";
                character.jobComponent.TriggerRoamAroundTile(out producedJob);
            }
        } else {
            log += "\n-Is not in the target npcSettlement";
            log += "\n-Roam there";
            HexTile targetHex = character.behaviourComponent.assignedTargetSettlement.tiles[UnityEngine.Random.Range(0, character.behaviourComponent.assignedTargetSettlement.tiles.Count)];
            LocationGridTile targetTile = targetHex.locationGridTiles[UnityEngine.Random.Range(0, targetHex.locationGridTiles.Count)];
            character.jobComponent.TriggerRoamAroundTile(out producedJob, targetTile);

        }
        return true;
    }
}
