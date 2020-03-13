using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class InvadeBehaviour : CharacterBehaviourComponent {
    public InvadeBehaviour() {
        priority = 0;
    }
    public override bool TryDoBehaviour(Character character, ref string log) {
        log += $"\n-{character.name} will invade";
        if (character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner && character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.settlementOnTile == character.behaviourComponent.harassInvadeRaidTarget) {
            log += "\n-Already in the target npcSettlement";
            log += "\n-Roam";
            character.jobComponent.TriggerRoamAroundTile();
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
