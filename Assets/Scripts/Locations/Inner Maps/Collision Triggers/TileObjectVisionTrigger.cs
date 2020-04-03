using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObjectVisionTrigger : POIVisionTrigger {

    public override void Initialize(IDamageable damageable) {
        base.Initialize(damageable);
        if (poi is GenericTileObject && projectileReceiver != null) {
            projectileReceiver.gameObject.SetActive(false);
        }
        TileObject tileObject = damageable as TileObject;
        if (tileObject.tileObjectType.IsTileObjectVisibleByDefault()) {
            VoteToMakeVisibleToCharacters();
        } 
        // else {
        //     VoteToMakeInvisibleToCharacters();    
        // }
        
    }
    public override bool IgnoresStructureDifference() {
        if (poi is MovingTileObject) {
            return true;
        }
        return false;
    }
    
}
