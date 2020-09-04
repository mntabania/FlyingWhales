using System.Collections.Generic;

public class Candelabra : TileObject{
    
    public Candelabra() {
        Initialize(TILE_OBJECT_TYPE.CANDELABRA);
    }
    public Candelabra(SaveDataTileObject data) : base(data) {
        
    }
}
