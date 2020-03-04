using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;

public class Ore : TileObject {
    public int yield { get; private set; }

    //private const int Supply_Per_Mine = 50;

    public Ore() {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.MINE_METAL, INTERACTION_TYPE.ASSAULT, };
        Initialize(TILE_OBJECT_TYPE.ORE);
        SetYield(50);
    }
    public Ore(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.MINE_METAL, INTERACTION_TYPE.ASSAULT, };
        Initialize(data);
    }

    #region Overrides
    public override string ToString() {
        return $"Ore {id}";
    }
    public override void SetPOIState(POI_STATE state) {
        base.SetPOIState(state);
        if (gridTileLocation != null && mapVisual != null) {
            mapVisual.UpdateTileObjectVisual(this); //update visual based on state
        }
    }
    #endregion

    //public int GetSupplyPerMine() {
    //    if (yield < Supply_Per_Mine) {
    //        return yield;
    //    }
    //    return Supply_Per_Mine;
    //}
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
    public override string GetAdditionalTestingData() {
        string data = base.GetAdditionalTestingData();
        data = $"{data}\n\tYield: {yield.ToString()}";
        return data;
    }
}

public class SaveDataOre : SaveDataTileObject {
    public int yield;

    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        Ore obj = tileObject as Ore;
        yield = obj.yield;
    }

    public override TileObject Load() {
        Ore obj = base.Load() as Ore;
        obj.SetYield(yield);
        return obj;
    }
}