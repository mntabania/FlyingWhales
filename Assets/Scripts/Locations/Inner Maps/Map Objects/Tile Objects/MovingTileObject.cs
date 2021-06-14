﻿using System;
using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
public abstract class MovingTileObject : TileObject {
    public sealed override LocationGridTile gridTileLocation => TryGetGridTileLocation(out var tile) ? tile : base.gridTileLocation;
    public override MapObjectVisual<TileObject> mapVisual => _mapVisual;
    private MovingMapObjectVisual _mapVisual;
    public bool hasExpired { get; protected set; }
    protected virtual int affectedRange => 1;

    public bool isPlayerSource { get; private set; }

    public override System.Type serializedData => typeof(SaveDataMovingTileObject);

    public MovingTileObject() : base() { }
    public MovingTileObject(SaveDataMovingTileObject data) : base(data) {
        isPlayerSource = data.isPlayerSource;
    }
    
    protected virtual bool TryGetGridTileLocation(out LocationGridTile tile) {
        if (_mapVisual != null) {
            if (_mapVisual.isSpawned) {
                tile = _mapVisual.gridTileLocation;
                return true;
            }
        }
        tile = null;
        return false;
    }

    #region Override Methods
    protected override void CreateMapObjectVisual() {
        GameObject obj = InnerMapManager.Instance.mapObjectFactory.CreateNewTileObjectMapVisual(this.tileObjectType);
        _mapVisual = obj.GetComponent<MovingMapObjectVisual>();
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        Messenger.AddListener<LocationGridTile, TraitableCallback>(GridTileSignals.ACTION_PERFORMED_ON_TILE_TRAITABLES, OnActionPerformedOnTile);
    }
    public virtual void Expire() {
        hasExpired = true;
        Messenger.RemoveListener<LocationGridTile, TraitableCallback>(GridTileSignals.ACTION_PERFORMED_ON_TILE_TRAITABLES, OnActionPerformedOnTile);
        DatabaseManager.Instance.tileObjectDatabase.UnRegisterTileObject(this);
    } 
    #endregion

    #region Listeners
    private void OnActionPerformedOnTile(LocationGridTile tile, TraitableCallback action) {
        if (affectedRange == 0) {
            if (tile == gridTileLocation) {
                action.Invoke(this);
            }  
        } else {
            List<LocationGridTile> affectedTiles = RuinarchListPool<LocationGridTile>.Claim(); 
            gridTileLocation.PopulateTilesInRadius(affectedTiles, affectedRange, includeCenterTile: true,
                includeTilesInDifferentStructure: true);
            if (affectedTiles.Contains(tile)) {
                action.Invoke(this);
            }
            RuinarchListPool<LocationGridTile>.Release(affectedTiles);
        }
        
    }
    #endregion

    public void SetIsPlayerSource(bool p_state) {
        isPlayerSource = p_state;
    }
}

#region Save Data
public class SaveDataMovingTileObject : SaveDataTileObject {
    public Vector3 mapVisualWorldPosition;
    public bool hasExpired;
    public bool isPlayerSource;
    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        MovingTileObject movingTileObject = tileObject as MovingTileObject;
        Assert.IsNotNull(movingTileObject);
        if (tileObject.mapObjectVisual != null) {
            mapVisualWorldPosition = tileObject.mapObjectVisual.transform.position;
        }
        hasExpired = movingTileObject.hasExpired;
        isPlayerSource = movingTileObject.isPlayerSource;
    }
}
#endregion