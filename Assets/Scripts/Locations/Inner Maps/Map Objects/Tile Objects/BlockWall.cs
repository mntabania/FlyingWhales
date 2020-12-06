using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;

public class BlockWall : TileObject {
    
    public WALL_TYPE wallType { get; private set; }
    public GameDate expiryDate { get; private set; }
    private string _expiryScheduleKey;

    #region Getters
    public string expiryScheduleKey => _expiryScheduleKey;
    public override System.Type serializedData => typeof(SaveDataBlockWall);
    #endregion
    
    public BlockWall() {
        Initialize(TILE_OBJECT_TYPE.BLOCK_WALL, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.DIG);
        traitContainer.RemoveTrait(this, "Flammable");
        traitContainer.AddTrait(this, "Immovable");
    }
    public BlockWall(SaveDataBlockWall data) : base(data) { }
    public void SetWallType(WALL_TYPE _wallType) {
        wallType = _wallType;
    }

    #region Overrides
    public override bool CanBeAffectedByElementalStatus(string traitName) {
        return false;
    }
    public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true, bool destroyTileSlots = true) {
        removedFrom.parentMap.structureTilemap.SetTile(removedFrom.localPlace, null);
        removedFrom.SetTileType(LocationGridTile.Tile_Type.Empty);
        mapVisual.DestroyExistingGUS();
        if (!string.IsNullOrEmpty(_expiryScheduleKey)) {
            SchedulingManager.Instance.RemoveSpecificEntry(_expiryScheduleKey);
        }
        base.OnRemoveTileObject(removedBy, removedFrom, removeTraits, destroyTileSlots);
    }
    protected override void OnPlaceTileObjectAtTile(LocationGridTile tile) {
        tile.parentMap.structureTilemap.SetTile(tile.localPlace, InnerMapManager.Instance.assetManager.GetWallAssetBasedOnWallType(wallType));
        tile.SetTileType(LocationGridTile.Tile_Type.Wall);
        Vector2 size = new Vector2(1.15f, 1.15f);
        if (wallType == WALL_TYPE.Flesh) {
            size = new Vector2(0.5f, 0.5f);
        } else if (wallType == WALL_TYPE.Demon_Stone) {
            size = new Vector2(1f, 1f);
        }
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

    #region Expiry
    public void SetExpiry(GameDate expiry) {
        expiryDate = expiry;
        _expiryScheduleKey = SchedulingManager.Instance.AddEntry(expiryDate, Expire, this);
    }
    private void Expire() {
        if (gridTileLocation != null) {
            gridTileLocation.structure.RemovePOI(this);    
        }
    }
    #endregion
    
    public void UpdateVisual(LocationGridTile tile) {
        tile.parentMap.structureTilemap.SetTile(tile.localPlace, InnerMapManager.Instance.assetManager.GetWallAssetBasedOnWallType(wallType));
    }
}

#region Save Data
public class SaveDataBlockWall : SaveDataTileObject {

    public WALL_TYPE wallType;
    public bool hasExpiry;
    public GameDate expiryDate;
    
    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        BlockWall blockWall = tileObject as BlockWall;
        Assert.IsNotNull(blockWall);
        wallType = blockWall.wallType;
        hasExpiry = !string.IsNullOrEmpty(blockWall.expiryScheduleKey);
        expiryDate = blockWall.expiryDate;
    }
    public override TileObject Load() {
        TileObject tileObject = base.Load();
        BlockWall blockWall = tileObject as BlockWall;
        Assert.IsNotNull(blockWall);
        blockWall.SetWallType(wallType);
        if (hasExpiry) {
            blockWall.SetExpiry(expiryDate);
        }
        return tileObject;
    }
}
#endregion
