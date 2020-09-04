using System.Collections.Generic;

public class Bandages : TileObject {
    public Bandages() {
        Initialize(TILE_OBJECT_TYPE.BANDAGES);
    }
    public Bandages(SaveDataTileObject data) : base(data) { }
}
