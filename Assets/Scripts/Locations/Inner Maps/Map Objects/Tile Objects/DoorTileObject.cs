using System;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;

public class DoorTileObject : TileObject {
    
    public bool isOpen { get; private set; }
    public override Type serializedData => typeof(SaveDataDoorTileObject);

    private DoorGameObject _doorGameObject;
    
    public DoorTileObject() {
        Initialize(TILE_OBJECT_TYPE.DOOR_TILE_OBJECT);
        RemoveAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
        traitContainer.AddTrait(this, "Immovable");
    }
    public DoorTileObject(SaveDataTileObject data) : base(data) { }

    #region Loading
    public override void LoadAdditionalInfo(SaveDataTileObject data) {
        base.LoadAdditionalInfo(data);
        SaveDataDoorTileObject saveDataDoorTileObject = data as SaveDataDoorTileObject;
        System.Diagnostics.Debug.Assert(saveDataDoorTileObject != null, nameof(saveDataDoorTileObject) + " != null");
        if (saveDataDoorTileObject.isOpen) {
            Open();
        } else {
            Close();  
        }
    }
    #endregion

    #region Overrides
    protected override void CreateMapObjectVisual() {
        base.CreateMapObjectVisual();
        _doorGameObject = mapVisual as DoorGameObject;
    }
    public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true, bool destroyTileSlots = true) {
        base.OnRemoveTileObject(removedBy, removedFrom, removeTraits, destroyTileSlots);
        mapVisual.DestroyExistingGUS();
    }
    protected override void OnPlaceTileObjectAtTile(LocationGridTile tile) {
        base.OnPlaceTileObjectAtTile(tile);
        mapVisual.InitializeGUS(Vector2.zero, Vector2.one, tile);
    }
    public override bool IsUnpassable() {
        return !isOpen;
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
    #endregion
    

    public void Open() {
        if (mapVisual == null) { return; }
#if DEBUG_LOG
        Debug.Log($"Opened door {this} at {structureLocation} {gridTileLocation}");
#endif
        isOpen = true;
        _doorGameObject.SetBlockerState(false);
        mapVisual.SetVisualAlpha(0f);
        mapVisual.DestroyExistingGUS();
    }
    public void Close() {
        if (mapVisual == null) { return; }
#if DEBUG_LOG
        Debug.Log($"Closed door {this} at {structureLocation} {gridTileLocation}");
#endif
        isOpen = false;
        _doorGameObject.SetBlockerState(true);
        mapVisual.SetVisualAlpha(1f);
        mapVisual.InitializeGUS(Vector2.zero, Vector2.one, gridTileLocation);
    }
}

#region Save Data
public class SaveDataDoorTileObject : SaveDataTileObject {
    public bool isOpen;
    public override void Save(TileObject data) {
        base.Save(data);
        DoorTileObject doorTileObject = data as DoorTileObject;
        Assert.IsNotNull(doorTileObject);
        isOpen = doorTileObject.isOpen;
    }
}
#endregion