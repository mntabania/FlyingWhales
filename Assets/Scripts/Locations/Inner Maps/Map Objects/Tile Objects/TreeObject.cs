using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;

public class TreeObject : TileObject {
    public int yield { get; private set; }
    
    public TreeObject() {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CHOP_WOOD, INTERACTION_TYPE.ASSAULT, INTERACTION_TYPE.REPAIR };
        Initialize(TILE_OBJECT_TYPE.TREE_OBJECT);
        SetYield(100);
    }
    public TreeObject(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CHOP_WOOD, INTERACTION_TYPE.ASSAULT, INTERACTION_TYPE.REPAIR };
        Initialize(data);
    }

    public override string ToString() {
        return $"Tree {id.ToString()}";
    }
    
    public void AdjustYield(int amount) {
        yield += amount;
        yield = Mathf.Max(0, yield);
        if (yield == 0 && gridTileLocation != null) {
            LocationGridTile loc = gridTileLocation;
            structureLocation.RemovePOI(this);
            SetGridTileLocation(loc); //so that it can still be targeted by aware characters.
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

public class SaveDataTreeObject: SaveDataTileObject {
    public int yield;

    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        TreeObject obj = tileObject as TreeObject;
        yield = obj.yield;
    }

    public override TileObject Load() {
        TreeObject obj = base.Load() as TreeObject;
        obj.SetYield(yield);
        return obj;
    }
}