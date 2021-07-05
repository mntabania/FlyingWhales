using System.Collections.Generic;

public class CounterTop : TileObject{
    public CounterTop() {
        Initialize(TILE_OBJECT_TYPE.COUNTER_TOP);
    }
    public CounterTop(SaveDataTileObject data) : base(data) {
        
    }
}