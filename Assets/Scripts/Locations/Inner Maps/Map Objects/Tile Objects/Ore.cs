using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;

public class Ore : TileObject {
    public int yield { get; private set; }
    public int count { get; set; }
    public override System.Type serializedData => typeof(SaveDataOre);
    
    public CONCRETE_RESOURCES providedMetal { get; private set; }
    
    public Ore() {
        Initialize(TILE_OBJECT_TYPE.ORE, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.MINE_METAL);
        AddAdvertisedAction(INTERACTION_TYPE.MINE_ORE);
        count = 120;
        SetYield(50);
    }
    public Ore(SaveDataOre data) : base(data) {
        yield = data.yield;
        providedMetal = data.providedMetal;
    }

    #region Overrides
    public override string ToString() {
        return $"Ore {id.ToString()}";
    }
    public override void SetPOIState(POI_STATE state) {
        base.SetPOIState(state);
        if (gridTileLocation != null && mapVisual != null) {
            mapVisual.UpdateTileObjectVisual(this); //update visual based on state
        }
    }
    #endregion

    #region Metal
    public void SetProvidedMetal(CONCRETE_RESOURCES p_providedMetal) {
        providedMetal = p_providedMetal;
    }
    #endregion
    
    public void AdjustYield(int amount) {
        yield += amount;
        yield = Mathf.Max(0, yield);
        if (yield == 0) {
            LocationGridTile loc = gridTileLocation;
            structureLocation.RemovePOI(this);
        }
    }
    private void SetYield(int amount) {
        yield = amount;
    }
    public override string GetAdditionalTestingData() {
        string data = base.GetAdditionalTestingData();
        data += $" <b>Count:</b> {count.ToString()}";
        data = $"{data}\n\tYield: {yield.ToString()}";
        return data;
    }
}

#region Save Data
public class SaveDataOre : SaveDataTileObject {
    public int yield;
    public int count;
    public CONCRETE_RESOURCES providedMetal;

    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        Ore obj = tileObject as Ore;
        yield = obj.yield;
        count = obj.count;
        providedMetal = obj.providedMetal;
    }

    public override TileObject Load() {
        Ore obj = base.Load() as Ore;
        obj.count = count;
        return obj;
    }
}
#endregion