using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inner_Maps;

public class Rock : TileObject{
    public int yield { get; private set; }

    public Rock() {
        Initialize(TILE_OBJECT_TYPE.ROCK);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.MINE_STONE, INTERACTION_TYPE.ASSAULT, };
        SetYield(50);
    }
    public Rock(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.MINE_STONE, INTERACTION_TYPE.ASSAULT, };
        Initialize(data);
    }

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
