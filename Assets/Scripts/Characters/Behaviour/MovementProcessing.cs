using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class MovementProcessing : CharacterBehaviourComponent {
    public MovementProcessing() {
        priority = 8;
    }

    public override bool TryDoBehaviour(Character character, ref string log) {
        if(character.gridTileLocation != null && character.gridTileLocation.collectionOwner.isPartOfParentRegionMap == false) {
            log += $"\n-{character.name} is in a grid tile location with no hex tile, must go to nearest hex tile";
            HexTile nearestHex = character.gridTileLocation.collectionOwner.GetNearestHexTileWithinRegion();
            character.jobComponent.TriggerMoveToHex(nearestHex);
            return true;
        }
        return false;
    }
}
