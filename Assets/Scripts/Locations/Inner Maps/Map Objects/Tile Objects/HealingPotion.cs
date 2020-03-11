using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingPotion : TileObject {

    public HealingPotion() {
        Initialize(TILE_OBJECT_TYPE.HEALING_POTION, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
        AddAdvertisedAction(INTERACTION_TYPE.SCRAP);
    }
    public HealingPotion(SaveDataTileObject data) {
        Initialize(data, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
        AddAdvertisedAction(INTERACTION_TYPE.SCRAP);
    }
}
