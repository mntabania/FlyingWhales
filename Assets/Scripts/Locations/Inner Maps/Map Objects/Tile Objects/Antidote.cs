﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Antidote : TileObject {

    public Antidote() {
        Initialize(TILE_OBJECT_TYPE.ANTIDOTE, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
        AddAdvertisedAction(INTERACTION_TYPE.SCRAP);
        AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
        AddAdvertisedAction(INTERACTION_TYPE.BOOBY_TRAP);
        AddAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
        // AddAdvertisedAction(INTERACTION_TYPE.STEAL);
    }

    public Antidote(SaveDataTileObject data) : base(data) { }
}
