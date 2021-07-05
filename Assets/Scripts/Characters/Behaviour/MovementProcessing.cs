using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class MovementProcessing : CharacterBehaviourComponent {
    public MovementProcessing() {
        priority = 27;
    }

    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        if(character.gridTileLocation != null && character.areaLocation == null) {
#if DEBUG_LOG
            log += $"\n-{character.name} is in a grid tile location with no hex tile, must go to nearest hex tile";
#endif
            Area nearestArea = character.gridTileLocation.GetNearestAreaWithinRegionThatCharacterHasPathTo(character);
            //HexTile nearestHex = character.gridTileLocation.collectionOwner.GetNearestHexTileWithinRegion();
            character.jobComponent.TriggerMoveToArea(out producedJob, nearestArea);
            return true;
        }
        producedJob = null;
        return false;
    }
}
