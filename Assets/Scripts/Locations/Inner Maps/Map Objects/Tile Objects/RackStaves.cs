using System.Collections.Generic;

public class RackStaves : TileObject{
    public RackStaves() {
        Initialize(TILE_OBJECT_TYPE.RACK_STAVES);
    }
    public RackStaves(SaveDataTileObject data) : base(data) {
        
    }
    protected override string GenerateName() { return "Staves"; }
}
