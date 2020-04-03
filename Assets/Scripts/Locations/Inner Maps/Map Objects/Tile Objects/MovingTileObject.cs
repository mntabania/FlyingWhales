using System;
using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UnityEngine;

public abstract class MovingTileObject : TileObject {
    public sealed override LocationGridTile gridTileLocation => TryGetGridTileLocation(out var tile) ? tile : base.gridTileLocation;
    public override MapObjectVisual<TileObject> mapVisual => _mapVisual;
    private MovingMapObjectVisual<TileObject> _mapVisual;
    public bool hasExpired { get; protected set; }
    protected virtual int affectedRange => 1;

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
        _mapVisual = obj.GetComponent<MovingMapObjectVisual<TileObject>>();
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        Messenger.AddListener<LocationGridTile, TraitableCallback>(Signals.ACTION_PERFORMED_ON_TILE_TRAITABLES, OnActionPerformedOnTile);
    }
    public virtual void Expire() {
        hasExpired = true;
        Messenger.RemoveListener<LocationGridTile, TraitableCallback>(Signals.ACTION_PERFORMED_ON_TILE_TRAITABLES, OnActionPerformedOnTile);
    } 
    #endregion

    #region Listeners
    private void OnActionPerformedOnTile(LocationGridTile tile, TraitableCallback action) {
        if (affectedRange == 0) {
            if (tile == gridTileLocation) {
                action.Invoke(this);
            }  
        } else {
            List<LocationGridTile> affectedTiles = gridTileLocation.GetTilesInRadius(affectedRange, includeCenterTile: true,
                includeTilesInDifferentStructure: true);
            if (affectedTiles.Contains(tile)) {
                action.Invoke(this);
            }    
        }
        
    }
    #endregion
}
