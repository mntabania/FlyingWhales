using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Guitar : TileObject {

    public Guitar() {
        //advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.PLAY_GUITAR, INTERACTION_TYPE.ASSAULT, INTERACTION_TYPE.REPAIR };
        Initialize(TILE_OBJECT_TYPE.GUITAR);
        AddAdvertisedAction(INTERACTION_TYPE.PLAY_GUITAR);
    }
    public Guitar(SaveDataTileObject data) {
        //advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.PLAY_GUITAR, INTERACTION_TYPE.ASSAULT, INTERACTION_TYPE.REPAIR };
        Initialize(data);
        AddAdvertisedAction(INTERACTION_TYPE.PLAY_GUITAR);
    }
    public override string ToString() {
        return $"Guitar {id}";
    }

    public virtual bool CanBeReplaced() {
        return true;
    }
}
