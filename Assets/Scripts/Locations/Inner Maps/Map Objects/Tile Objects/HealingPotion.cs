using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingPotion : TileObject {

    public HealingPotion() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.HEALING_POTION);
        //RemoveCommonAdvertisements();
        RemoveAdvertisedAction(INTERACTION_TYPE.REPAIR);
    }
    public HealingPotion(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
