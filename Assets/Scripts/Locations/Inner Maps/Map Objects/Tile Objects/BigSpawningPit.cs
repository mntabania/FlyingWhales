using UnityEngine;

public class BigSpawningPit : TileObject{
    
    public override Vector2 selectableSize => new Vector2(1.7f, 1.7f);
    public override Vector3 worldPosition {
        get {
            Vector3 pos = mapVisual.transform.position;
            pos.x += 0.5f;
            pos.y += 0.5f;
            return pos;
        }
    }
    
    public BigSpawningPit() {
        Initialize(TILE_OBJECT_TYPE.BIG_SPAWNING_PIT);
    }
    public BigSpawningPit(SaveDataTileObject data) : base(data) {
    }
}