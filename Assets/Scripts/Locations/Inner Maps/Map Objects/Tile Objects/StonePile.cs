using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StonePile : ResourcePile {

    public StonePile() : base(RESOURCE.STONE) {
        Initialize(TILE_OBJECT_TYPE.STONE_PILE, false);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        //SetResourceInPile(50);
        traitContainer.RemoveTrait(this, "Flammable");
        SetResourceInPile(100);
    }
    //public StonePile(SaveDataTileObject data) : base(RESOURCE.STONE) {
    //    Initialize(data, false);
    //    //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    //}

    //public override void AdjustResourceInPile(int adjustment) {
    //    base.AdjustResourceInPile(adjustment);
    //    if (adjustment < 0) {
    //        Messenger.Broadcast(Signals.STONE_IN_PILE_REDUCED, this);
    //    }
    //}
    public override string ToString() {
        return $"Stone Pile {id}";
    }
    public override bool CanBeReplaced() {
        return true;
    }
}

//public class SaveDataStonePile : SaveDataTileObject {
//    public int suppliesInPile;

//    public override void Save(TileObject tileObject) {
//        base.Save(tileObject);
//        StonePile obj = tileObject as StonePile;
//        suppliesInPile = obj.resourceInPile;
//    }

//    public override TileObject Load() {
//        StonePile obj = base.Load() as StonePile;
//        obj.SetResourceInPile(suppliesInPile);
//        return obj;
//    }
//}