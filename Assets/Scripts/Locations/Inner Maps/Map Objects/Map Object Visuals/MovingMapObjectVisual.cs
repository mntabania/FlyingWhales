using Inner_Maps;
using UnityEngine;

/// <summary>
/// Base Class for map objects that can move dynamically on their own.
/// </summary>
/// <typeparam name="T">The type of object that uses this.</typeparam>
public abstract class MovingMapObjectVisual : MapObjectVisual<TileObject>{
    
    public bool isSpawned { get; protected set; }
    public LocationGridTile gridTileLocation => GetLocationGridTileByXy(Mathf.FloorToInt(localPos.x), Mathf.FloorToInt(localPos.y));
    protected Region _mapLocation;
    public Vector3 worldPos { get; private set; }
    public Vector3 localPos { get; private set; }

    public override void Initialize(TileObject obj) {
        base.Initialize(obj);
        _mapLocation = obj.gridTileLocation.parentMap.region;
    }
    public override void PlaceObjectAt(LocationGridTile tile) {
        base.PlaceObjectAt(tile);
        localPos = transform.localPosition;
        worldPos = transform.position;
    }
    public override void Reset() {
        base.Reset();
        _mapLocation = null;
        obj = null;
    }
    private LocationGridTile GetLocationGridTileByXy(int x, int y) {
        if (UtilityScripts.Utilities.IsInRange(x, 0, _mapLocation.innerMap.width) 
            && UtilityScripts.Utilities.IsInRange(y, 0, _mapLocation.innerMap.height)) {
            return _mapLocation.innerMap.map[x, y];    
        }
        return null;
    }
    protected virtual void Update() {
        localPos = transform.localPosition;
        worldPos = transform.position;
    }
    
}
