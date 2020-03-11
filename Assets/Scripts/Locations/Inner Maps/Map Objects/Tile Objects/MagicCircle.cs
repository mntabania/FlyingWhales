using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MagicCircle : TileObject {
    public MagicCircle() {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT, };
        Initialize(TILE_OBJECT_TYPE.MAGIC_CIRCLE);
    }
    public MagicCircle(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT, };
        Initialize(data);
    }

    public override string ToString() {
        return $"Magic Circle {id}";
    }
}
