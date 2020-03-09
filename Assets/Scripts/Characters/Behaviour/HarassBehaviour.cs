using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class HarassBehaviour : CharacterBehaviourComponent {
    public HarassBehaviour() {
        priority = 0;
    }
    public override bool TryDoBehaviour(Character character, ref string log) {
        log += $"\n-{character.name} will harass";
        if (character.gridTileLocation.buildSpotOwner.hexTileOwner && character.gridTileLocation.buildSpotOwner.hexTileOwner.settlementOnTile == character.behaviourComponent.harassInvadeRaidTarget) {
            log += "\n-Already in the target npcSettlement";
            if(character.marker.inVisionTileObjects.Count > 0) {
                log += "\n-Has tile object in vision";
                log += "\n-Adding tile object as hostile";
                character.combatComponent.Fight(character.marker.inVisionTileObjects[0]);
            } else {
                log += "\n-No tile object in vision";
                log += "\n-Roam";
                character.jobComponent.TriggerRoamAroundTile();
            }
        } else {
            log += "\n-Is not in the target npcSettlement";
            log += "\n-Roam there";
            HexTile targetHex = character.behaviourComponent.harassInvadeRaidTarget.tiles[UnityEngine.Random.Range(0, character.behaviourComponent.harassInvadeRaidTarget.tiles.Count)];
            LocationGridTile targetTile = targetHex.locationGridTiles[UnityEngine.Random.Range(0, targetHex.locationGridTiles.Count)];
            character.jobComponent.TriggerRoamAroundTile(targetTile);

        }
        return true;
    }
}
