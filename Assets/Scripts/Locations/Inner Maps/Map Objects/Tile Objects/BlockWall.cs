using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BlockWall : TileObject {
    
    public WALL_TYPE wallType { get; private set; }
    
    public BlockWall() {
        Initialize(TILE_OBJECT_TYPE.BLOCK_WALL, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        //advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        traitContainer.RemoveTrait(this, "Flammable");
        traitContainer.AddTrait(this, "Immovable");
    }
    public BlockWall(SaveDataTileObject data) {
        Initialize(data, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        //advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        traitContainer.RemoveTrait(this, "Flammable");
        traitContainer.AddTrait(this, "Immovable");
    }
    public void SetWallType(WALL_TYPE _wallType) {
        wallType = _wallType;
    }

    #region Overrides
    public override bool CanBeAffectedByElementalStatus(string traitName) {
        return false;
    }
    public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true,
        bool destroyTileSlots = true) {
        removedFrom.parentMap.structureTilemap.SetTile(removedFrom.localPlace, null);
        removedFrom.SetTileType(LocationGridTile.Tile_Type.Empty);
        mapVisual.DestroyExistingGUS();
        base.OnRemoveTileObject(removedBy, removedFrom, removeTraits, destroyTileSlots);
    }
    protected override void OnPlaceTileObjectAtTile(LocationGridTile tile) {
        tile.parentMap.structureTilemap.SetTile(tile.localPlace, InnerMapManager.Instance.assetManager.GetWallAssetBasedOnWallType(wallType));
        tile.SetTileType(LocationGridTile.Tile_Type.Wall);
        mapVisual.InitializeGUS(Vector2.zero, wallType == WALL_TYPE.Flesh ? new Vector2(0.5f, 0.5f) : Vector2.one);

        base.OnPlaceTileObjectAtTile(tile);
    }
    public override bool CollectsLogs() {
        return false;
    }
    #endregion

    public void UpdateVisual(LocationGridTile tile) {
        tile.parentMap.structureTilemap.SetTile(tile.localPlace, InnerMapManager.Instance.assetManager.GetWallAssetBasedOnWallType(wallType));
    }
}
