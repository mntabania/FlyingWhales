using Inner_Maps;
using Inner_Maps.Location_Structures;
using Inner_Maps.Map_Objects.Map_Object_Visuals;
using Locations.Settlements;
using UnityEngine;

public class OreVein : TileObject {
    public override StructureConnector structureConnector => _oreVeinGameObject.structureConnector;
    private OreVeinGameObject _oreVeinGameObject;
    
    public OreVein() {
        Initialize(TILE_OBJECT_TYPE.ORE_VEIN);
        BaseSettlement.onSettlementBuilt += UpdateSettlementResourcesParent;
    }
    public OreVein(SaveDataTileObject data) { }
    
    public override void UpdateSettlementResourcesParent() {
        BaseSettlement.onSettlementBuilt -= UpdateSettlementResourcesParent;
        if (gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
            gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.AllNeighbours.ForEach((eachNeighboringHexTile) => {
                if (eachNeighboringHexTile.settlementOnTile != null) {
                    if (!eachNeighboringHexTile.settlementOnTile.SettlementResources.oreVeins.Contains(this)) {
                        eachNeighboringHexTile.settlementOnTile.SettlementResources.oreVeins.Add(this);
                        parentSettlement = eachNeighboringHexTile.settlementOnTile;
                    }
                }
            });
        }
    }

    public override void RemoveFromSettlementResourcesParent() {
        if (parentSettlement != null) {
            parentSettlement.SettlementResources.oreVeins.Remove(this);
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
        tile.parentMap.structureTilemap.SetColor(tile.localPlace, Color.cyan);
        tile.SetTileType(LocationGridTile.Tile_Type.Wall);
        Vector2 size = new Vector2(1.15f, 1.15f);
        mapVisual.InitializeGUS(Vector2.zero, size, tile);

        base.OnPlaceTileObjectAtTile(tile);
    }
    public override bool CollectsLogs() {
        return false;
    }
    public override void ConstructDefaultActions() {
        base.ConstructDefaultActions();
        RemovePlayerAction(PLAYER_SKILL_TYPE.SEIZE_OBJECT);
        RemovePlayerAction(PLAYER_SKILL_TYPE.POISON);
        RemovePlayerAction(PLAYER_SKILL_TYPE.IGNITE);
    }
    #endregion

}