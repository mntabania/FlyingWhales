using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TileObjectVisionTrigger : POIVisionTrigger {

    public override void Initialize(IDamageable damageable) {
        base.Initialize(damageable);
        if (poi is GenericTileObject && projectileReceiver != null) {
            projectileReceiver.gameObject.SetActive(false);
        }
        TileObject tileObject = damageable as TileObject;
        
        //recompute filter votes upon initialization, this is to ensure the proper value.
        int votes = 0;
        if (tileObject.tileObjectType.IsTileObjectVisibleByDefault()) {
            votes++;
        }
        if (tileObject.lastManipulatedBy is Player) {
            votes++;
        }
        votes += tileObject.allJobsTargetingThis.Count;
        votes += tileObject.traitContainer.statuses.Count(s => s.isTangible);
        SetFilterVotes(votes);

    }
    public override bool IgnoresStructureDifference() {
        if (poi is MovingTileObject) {
            return true;
        }
        if (poi is TileObject tileObject && tileObject.tileObjectType.IsDemonicStructureTileObject()) {
            return true;
        }
        return false;
    }
    public override bool IgnoresRoomDifference() {
        if (poi is TileObject tileObject && tileObject.tileObjectType.IsDemonicStructureTileObject()) {
            return true;
        }
        return false;
    }
}
