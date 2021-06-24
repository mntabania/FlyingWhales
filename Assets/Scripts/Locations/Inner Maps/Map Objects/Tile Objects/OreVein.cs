using Inner_Maps;
using Inner_Maps.Location_Structures;
using Inner_Maps.Map_Objects.Map_Object_Visuals;
using Locations.Settlements;
using UnityEngine;

public class OreVein : TileObject {
    // public StructureConnector structureConnector {
    //     get {
    //         if (_oreVeinGameObject != null) {
    //             return _oreVeinGameObject.structureConnector;
    //         }
    //         return null;
    //     }
    // }
    // public BaseSettlement parentSettlement { get; private set; }
    // private OreVeinGameObject _oreVeinGameObject;
    public OreVein() {
        Initialize(TILE_OBJECT_TYPE.ORE_VEIN);
        // BaseSettlement.onSettlementBuilt += UpdateSettlementResourcesParent;
    }
    public OreVein(SaveDataTileObject data) : base(data) { }
    // protected override void UpdateSettlementResourcesParent() {
    //     if (gridTileLocation.area.settlementOnArea != null) {
    //         gridTileLocation.area.settlementOnArea.SettlementResources?.AddToListBasedOnRequirement(SettlementResources.StructureRequirement.MINE_SHACK_SPOT, this);
    //     }
    //     gridTileLocation.area.neighbourComponent.neighbours.ForEach((eachNeighbor) => {
    //         if (eachNeighbor.settlementOnArea != null) {
    //             eachNeighbor.settlementOnArea.SettlementResources?.AddToListBasedOnRequirement(SettlementResources.StructureRequirement.MINE_SHACK_SPOT, this);
    //             parentSettlement = eachNeighbor.settlementOnArea;
    //         }
    //     });
    // }
    // protected override void RemoveFromSettlementResourcesParent() {
    //     if (parentSettlement != null && parentSettlement.SettlementResources != null) {
    //         if (parentSettlement.SettlementResources.mineShackSpots.Remove(this)) {
    //             parentSettlement = null;
    //         }
    //     }
    // }
    
    #region Overrides
    // protected override void CreateMapObjectVisual() {
    //     base.CreateMapObjectVisual();
    //     _oreVeinGameObject = mapVisual as OreVeinGameObject;
    // }
    // public override void DestroyMapVisualGameObject() {
    //     base.DestroyMapVisualGameObject();
    //     _oreVeinGameObject = null;
    // }
    public override bool CanBeAffectedByElementalStatus(string traitName) {
        return false;
    }
    public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true, bool destroyTileSlots = true) {
        removedFrom.SetWallTilemapTileAsset(null);
        //removedFrom.parentMap.structureTilemap.SetTile(removedFrom.localPlace, null);
        removedFrom.SetTileType(LocationGridTile.Tile_Type.Empty);
        mapVisual.DestroyExistingGUS();
        base.OnRemoveTileObject(removedBy, removedFrom, removeTraits, destroyTileSlots);
    }
    protected override void OnPlaceTileObjectAtTile(LocationGridTile tile) {
        tile.SetWallTilemapTileAsset(InnerMapManager.Instance.assetManager.GetWallAssetBasedOnWallType(WALL_TYPE.Stone));
        //tile.parentMap.structureTilemap.SetTile(tile.localPlace, InnerMapManager.Instance.assetManager.GetWallAssetBasedOnWallType(WALL_TYPE.Stone));
        // tile.parentMap.structureTilemap.SetColor(tile.localPlace, Color.cyan);
        tile.SetTileType(LocationGridTile.Tile_Type.Wall);
        Vector2 size = new Vector2(1.15f, 1.15f);
        mapVisual.InitializeGUS(Vector2.zero, size, tile);
        mapVisual.UpdateTileObjectVisual(this);

        // if (structureConnector != null) {
        //     structureConnector.OnPlaceConnector(tile.parentMap);    
        // }

        base.OnPlaceTileObjectAtTile(tile);
    }
    public override void ConstructDefaultActions() {
        base.ConstructDefaultActions();
        RemovePlayerAction(PLAYER_SKILL_TYPE.SEIZE_OBJECT);
        RemovePlayerAction(PLAYER_SKILL_TYPE.POISON);
        RemovePlayerAction(PLAYER_SKILL_TYPE.IGNITE);
    }
    // public override void OnPlacePOI() {
    //     base.OnPlacePOI();
    //     UpdateSettlementResourcesParent();
    // }
    // public override void OnDestroyPOI() {
    //     base.OnDestroyPOI();
    //     BaseSettlement.onSettlementBuilt -= UpdateSettlementResourcesParent;
    // }
    public override bool IsUnpassable() {
        return true;
    }
    public override bool IsValidCombatTargetFor(IPointOfInterest source) {
        if (gridTileLocation == null) {
            return false;
        }
        if (source.gridTileLocation == null) {
            return false;
        }
        return true;
    }
    public override void LoadAdditionalInfo(SaveDataTileObject data) {
        if (mapVisual != null) {
            //since ore veins need to updated after all cave walls have been placed
            mapVisual.UpdateTileObjectVisual(this);
        }
    }
    #endregion

}