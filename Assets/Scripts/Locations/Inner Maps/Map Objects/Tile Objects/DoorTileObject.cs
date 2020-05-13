using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class DoorTileObject : TileObject {
    public DoorTileObject() {
        Initialize(TILE_OBJECT_TYPE.DOOR_TILE_OBJECT);
    }
    public DoorTileObject(SaveDataTileObject data) {
        Initialize(data);
    }
    
    public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true,
        bool destroyTileSlots = true) {
        base.OnRemoveTileObject(removedBy, removedFrom, removeTraits, destroyTileSlots);
        mapVisual.DestroyExistingGUS();
    }
    protected override void OnPlaceTileObjectAtTile(LocationGridTile tile) {
        base.OnPlaceTileObjectAtTile(tile);
        mapVisual.InitializeGUS(Vector2.zero, Vector2.one);
    }

    public void Open() {
        if (mapVisual == null) { return; }
        mapVisual.SetVisualAlpha(0f);
        mapVisual.DestroyExistingGUS();
    }
    public void Close() {
        if (mapVisual == null) { return; }
        mapVisual.SetVisualAlpha(1f);
        mapVisual.InitializeGUS(Vector2.zero, Vector2.one);
    }
}