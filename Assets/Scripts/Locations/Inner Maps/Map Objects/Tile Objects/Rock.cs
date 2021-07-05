using System;
using UnityEngine;
using Inner_Maps;
using Locations.Settlements;
using Inner_Maps.Location_Structures;
using Inner_Maps.Map_Objects.Map_Object_Visuals;

public class Rock : TileObject{
    public int yield { get; private set; }

    public int count { get; set; }
    public override Type serializedData => typeof(SaveDataRock);
    // public StructureConnector structureConnector {
    //     get {
    //         if (_rockGameObject != null) {
    //             return _rockGameObject.structureConnector;
    //         }
    //         return null;
    //     }
    // }
    //public BaseSettlement parentSettlement { get; private set; }
    
    // private RockGameObject _rockGameObject;
    public Rock() {
        Initialize(TILE_OBJECT_TYPE.ROCK, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.MINE_STONE);
        count = 100;
        SetYield(50);
        //BaseSettlement.onSettlementBuilt += UpdateSettlementResourcesParent;
    }
    public Rock(SaveDataTileObject data) : base(data) { }
    // protected override void CreateMapObjectVisual() {
    //     base.CreateMapObjectVisual();
    //     _rockGameObject = mapVisual as RockGameObject;
    // }
    // public override void DestroyMapVisualGameObject() {
    //     base.DestroyMapVisualGameObject();
    //     _rockGameObject = null;
    // }
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
    //protected override void UpdateSettlementResourcesParent() {
    //    if (gridTileLocation != null) {
    //        if (gridTileLocation.area.settlementOnArea != null) {
    //            gridTileLocation.area.settlementOnArea.SettlementResources?.AddToListBasedOnRequirement(SettlementResources.StructureRequirement.ROCK, this);
    //        }
    //        gridTileLocation.area.neighbourComponent.neighbours.ForEach((eachNeighbor) => {
    //            if (eachNeighbor.settlementOnArea != null) {
    //                //eachNeighbor.settlementOnArea.SettlementResources?.AddToListBasedOnRequirement(SettlementResources.StructureRequirement.ROCK, this);
    //                parentSettlement = eachNeighbor.settlementOnArea;
    //            }
    //        });
    //    }
    //}
    //protected override void RemoveFromSettlementResourcesParent() {
    //    if (parentSettlement != null && parentSettlement.SettlementResources != null) {
    //        if (parentSettlement.SettlementResources.rocks.Remove(this)) {
    //            parentSettlement = null;
    //        }
    //    }
    //}

    public override string GetAdditionalTestingData() {
        string data = base.GetAdditionalTestingData();
        return data += $" <b>Count:</b> {count.ToString()}";
    }

    //public override void OnPlacePOI() {
    //    base.OnPlacePOI();
    //    // if (structureConnector != null && gridTileLocation != null) {
    //    //     structureConnector.OnPlaceConnector(gridTileLocation.parentMap);    
    //    // }
    //    UpdateSettlementResourcesParent();
    //}
    //public override void OnDestroyPOI() {
    //    base.OnDestroyPOI();
    //    BaseSettlement.onSettlementBuilt -= UpdateSettlementResourcesParent;
    //}
}
#region Save Data
public class SaveDataRock : SaveDataTileObject {
    public int yield;
    public int count;
    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        Rock obj = tileObject as Rock;
        yield = obj.yield;
        count = obj.count;
    }

    public override TileObject Load() {
        Rock obj = base.Load() as Rock;
        obj.SetYield(yield);
        obj.count = count;
        return obj;
    }
}
#endregion