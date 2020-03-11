using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Guitar : TileObject {

    public Guitar() {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.PLAY_GUITAR, INTERACTION_TYPE.ASSAULT, INTERACTION_TYPE.REPAIR };
        Initialize(TILE_OBJECT_TYPE.GUITAR);
    }
    public Guitar(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.PLAY_GUITAR, INTERACTION_TYPE.ASSAULT, INTERACTION_TYPE.REPAIR };
        Initialize(data);
    }
    public override string ToString() {
        return $"Guitar {id}";
    }

    public override bool CanBeReplaced() {
        return true;
    }
}
