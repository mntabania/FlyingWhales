using Inner_Maps;
using Inner_Maps.Location_Structures;
using Inner_Maps.Map_Objects.Map_Object_Visuals;
using Locations.Settlements;
using UnityEngine;

public class OreVein : TileObject {
    public override StructureConnector structureConnector {
        get {
            if (_oreVeinGameObject != null) {
                return _oreVeinGameObject.structureConnector;
            }
            return null;
        }
    }
    private OreVeinGameObject _oreVeinGameObject;
    
    public OreVein() {
        Initialize(TILE_OBJECT_TYPE.ORE_VEIN);
        BaseSettlement.onSettlementBuilt += UpdateSettlementResourcesParent;
    }
    public OreVein(SaveDataTileObject data) { }
    
    public override void UpdateSettlementResourcesParent() {
        if (gridTileLocation.parentArea.settlementOnTile != null) {
            gridTileLocation.parentArea.settlementOnTile.SettlementResources?.AddToListbaseOnRequirement(SettlementResources.StructureRequirement.ORE_VEIN, this);
        }
        gridTileLocation.parentArea.AllNeighbours.ForEach((eachNeighboringHexTile) => {
            if (eachNeighboringHexTile.settlementOnTile != null) {
                eachNeighboringHexTile.settlementOnTile.SettlementResources?.AddToListbaseOnRequirement(SettlementResources.StructureRequirement.ORE_VEIN, this);
                parentSettlement = eachNeighboringHexTile.settlementOnTile;
            }
        });
    }

    public override void RemoveFromSettlementResourcesParent() {
        if (parentSettlement != null && parentSettlement.SettlementResources != null) {
            if (parentSettlement.SettlementResources.oreVeins.Remove(this)) {
                parentSettlement = null;
            }
        }
    }
    
    #region Overrides
    protected override void CreateMapObjectVisual() {
        base.CreateMapObjectVisual();
        _oreVeinGameObject = mapVisual as OreVeinGameObject;
    }
    public override void DestroyMapVisualGameObject() {
        base.DestroyMapVisualGameObject();
        _oreVeinGameObject = null;
    }
    public override bool CanBeAffectedByElementalStatus(string traitName) {
        return false;
    }
    public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true, bool destroyTileSlots = true) {
        removedFrom.parentMap.structureTilemap.SetTile(removedFrom.localPlace, null);
        removedFrom.SetTileType(LocationGridTile.Tile_Type.Empty);
        mapVisual.DestroyExistingGUS();
        base.OnRemoveTileObject(removedBy, removedFrom, removeTraits, destroyTileSlots);
    }
    protected override void OnPlaceTileObjectAtTile(LocationGridTile tile) {
        tile.parentMap.structureTilemap.SetTile(tile.localPlace, InnerMapManager.Instance.assetManager.GetWallAssetBasedOnWallType(WALL_TYPE.Stone));
        // tile.parentMap.structureTilemap.SetColor(tile.localPlace, Color.cyan);
        tile.SetTileType(LocationGridTile.Tile_Type.Wall);
        Vector2 size = new Vector2(1.15f, 1.15f);
        mapVisual.InitializeGUS(Vector2.zero, size, tile);
        mapVisual.UpdateTileObjectVisual(this);

        base.OnPlaceTileObjectAtTile(tile);
    }
    public override void ConstructDefaultActions() {
        base.ConstructDefaultActions();
        RemovePlayerAction(PLAYER_SKILL_TYPE.SEIZE_OBJECT);
        RemovePlayerAction(PLAYER_SKILL_TYPE.POISON);
        RemovePlayerAction(PLAYER_SKILL_TYPE.IGNITE);
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        UpdateSettlementResourcesParent();
    }
    public override void OnDestroyPOI() {
        base.OnDestroyPOI();
        BaseSettlement.onSettlementBuilt -= UpdateSettlementResourcesParent;
    }
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