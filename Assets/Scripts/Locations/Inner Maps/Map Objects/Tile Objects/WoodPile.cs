﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WoodPile : ResourcePile {

    public WoodPile() : base(RESOURCE.WOOD) {
        Initialize(TILE_OBJECT_TYPE.WOOD_PILE);
        //SetResourceInPile(50);
        traitContainer.RemoveTrait(this, "Flammable");
    }
    public WoodPile(SaveDataTileObject data) : base(RESOURCE.WOOD) {
        Initialize(data);
    }
    //public override void AdjustResourceInPile(int adjustment) {
    //    base.AdjustResourceInPile(adjustment);
    //    if (adjustment < 0) {
    //        Messenger.Broadcast(Signals.WOOD_IN_PILE_REDUCED, this);
    //    }
    //}
    public override string ToString() {
        return $"Wood Pile {id}";
    }
    public override bool CanBeReplaced() {
        return true;
    }
}

public class SaveDataWoodPile : SaveDataTileObject {
    public int suppliesInPile;

    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        WoodPile obj = tileObject as WoodPile;
        suppliesInPile = obj.resourceInPile;
    }

    public override TileObject Load() {
        WoodPile obj = base.Load() as WoodPile;
        obj.SetResourceInPile(suppliesInPile);
        return obj;
    }
}