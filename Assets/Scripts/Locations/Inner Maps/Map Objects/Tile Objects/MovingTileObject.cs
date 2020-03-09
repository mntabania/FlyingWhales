using Inner_Maps;
using UnityEngine;

public abstract class MovingTileObject : TileObject {
    public sealed override LocationGridTile gridTileLocation => TryGetGridTileLocation(out var tile) ? tile : base.gridTileLocation;
    public override MapObjectVisual<TileObject> mapVisual => _mapVisual;
    private MovingMapObjectVisual<TileObject> _mapVisual;

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
    #endregion
}
