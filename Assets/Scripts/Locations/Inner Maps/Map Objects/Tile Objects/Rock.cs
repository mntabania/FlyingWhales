using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inner_Maps;

public class Rock : TileObject{
    public int yield { get; private set; }
    public override Type serializedData => typeof(SaveDataRock);
    public Rock() {
        Initialize(TILE_OBJECT_TYPE.ROCK, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.MINE_STONE);

        SetYield(50);
    }
    public Rock(SaveDataTileObject data) { }

    public void AdjustYield(int amount) {
        yield += amount;
        yield = Mathf.Max(0, yield);
        if (yield == 0) {
            LocationGridTile loc = gridTileLocation;
            structureLocation.RemovePOI(this);
            SetGridTileLocation(loc); //so that it can still be targetted by aware characters.
        }
    }
    public void SetYield(int amount) {
        yield = amount;
    }
}
#region Save Data
public class SaveDataRock : SaveDataTileObject {
    public int yield;

    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        Rock obj = tileObject as Rock;
        yield = obj.yield;
    }

    public override TileObject Load() {
        Rock obj = base.Load() as Rock;
        obj.SetYield(yield);
        return obj;
    }
}
#endregion