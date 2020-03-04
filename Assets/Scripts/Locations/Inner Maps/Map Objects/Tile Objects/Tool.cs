using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool : TileObject {

    public Tool() {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(TILE_OBJECT_TYPE.TOOL);
        //RemoveCommonAdvertisements();
        RemoveAdvertisedAction(INTERACTION_TYPE.REPAIR);
    }
    public Tool(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
