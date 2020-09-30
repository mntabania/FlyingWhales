using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingPotion : TileObject {

    public HealingPotion() {
        Initialize(TILE_OBJECT_TYPE.HEALING_POTION, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
        AddAdvertisedAction(INTERACTION_TYPE.SCRAP);
        AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
        AddAdvertisedAction(INTERACTION_TYPE.BOOBY_TRAP);

        // traitContainer.AddTrait(this, "Poisoned", overrideDuration: 0); //NOTE:THIS IS FOR TESTING PURPOSES ONLY AND SHOULD BE REMOVED ONCE TESTING IS DONE!
    }
    public HealingPotion(SaveDataTileObject data) { }
}
