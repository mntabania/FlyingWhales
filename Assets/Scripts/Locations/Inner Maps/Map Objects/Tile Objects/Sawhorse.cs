using System.Collections.Generic;

public class Sawhorse : TileObject{
    public Sawhorse() {
        Initialize(TILE_OBJECT_TYPE.SAWHORSE);
    }
    public Sawhorse(SaveDataTileObject data) : base(data) {
        
    }
}