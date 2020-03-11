using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool : TileObject {

    public Tool() {
        //advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(TILE_OBJECT_TYPE.TOOL, false);
        //RemoveCommonAdvertisements();
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
        AddAdvertisedAction(INTERACTION_TYPE.SCRAP);
    }
    public Tool(SaveDataTileObject data) {
        //advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
        AddAdvertisedAction(INTERACTION_TYPE.SCRAP);
    }
}
