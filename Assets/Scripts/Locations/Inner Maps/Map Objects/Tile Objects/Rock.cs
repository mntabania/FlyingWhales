using System;
using UnityEngine;
using Inner_Maps;
using Locations.Settlements;
using Inner_Maps.Location_Structures;
using Inner_Maps.Map_Objects.Map_Object_Visuals;

public class Rock : TileObject{
    public int yield { get; private set; }
    public override Type serializedData => typeof(SaveDataRock);
    public override StructureConnector structureConnector => _rockGameObject.structureConnector;
    
    private RockGameObject _rockGameObject;
    public Rock() {
        Initialize(TILE_OBJECT_TYPE.ROCK, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.MINE_STONE);

        SetYield(50);
        BaseSettlement.onSettlementBuilt += UpdateSettlementResourcesParent;
    }
    public Rock(SaveDataTileObject data) { }
    protected override void CreateMapObjectVisual() {
        base.CreateMapObjectVisual();
        _rockGameObject = mapVisual as RockGameObject;
    }
    public override void DestroyMapVisualGameObject() {
        base.DestroyMapVisualGameObject();
        _rockGameObject = null;
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

    public override void UpdateSettlementResourcesParent() {
        if (gridTileLocation != null && gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
            if (gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.settlementOnTile != null) {
                gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.settlementOnTile.SettlementResources.AddToListbaseOnRequirement(SettlementResources.StructureRequirement.ROCK, this);
            }
            gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.AllNeighbours.ForEach((eachNeighboringHexTile) => {
                if (eachNeighboringHexTile.settlementOnTile != null) {
                    eachNeighboringHexTile.settlementOnTile.SettlementResources.AddToListbaseOnRequirement(SettlementResources.StructureRequirement.ROCK, this);
                    parentSettlement = eachNeighboringHexTile.settlementOnTile;
                }
            });
        }
    }
    public override void RemoveFromSettlementResourcesParent() {
        if (parentSettlement != null) {
            parentSettlement.SettlementResources.rocks.Remove(this);
        }
    }

    public override void OnPlacePOI() {
        base.OnPlacePOI();
        UpdateSettlementResourcesParent();
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