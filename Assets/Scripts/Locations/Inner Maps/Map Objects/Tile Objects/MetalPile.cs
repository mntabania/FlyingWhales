using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class MetalPile : ResourcePile {

    public MetalPile(TILE_OBJECT_TYPE tileObjectType) : base(RESOURCE.METAL) {
        Initialize(tileObjectType, false);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        //SetResourceInPile(50);
        SetResourceInPile(100);
    }
    public MetalPile(SaveDataTileObject data) : base(data, RESOURCE.METAL) { }

    //public override void AdjustResourceInPile(int adjustment) {
    //    base.AdjustResourceInPile(adjustment);
    //    if (adjustment < 0) {
    //        Messenger.Broadcast(Signals.METAL_IN_PILE_REDUCED, this);
    //    }
    //}
    public override string ToString() {
        return $"Metal Pile {id.ToString()}";
    }
}

//public class SaveDataMetalPile : SaveDataTileObject {
//    public int suppliesInPile;

//    public override void Save(TileObject tileObject) {
//        base.Save(tileObject);
//        MetalPile obj = tileObject as MetalPile;
//        suppliesInPile = obj.resourceInPile;
//    }

//    public override TileObject Load() {
//        MetalPile obj = base.Load() as MetalPile;
//        obj.SetResourceInPile(suppliesInPile);
//        return obj;
//    }
//}