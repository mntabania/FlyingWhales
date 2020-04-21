using System.Collections.Generic;

public class RackStaves : TileObject{
    public RackStaves() {
        Initialize(TILE_OBJECT_TYPE.RACK_STAVES);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public RackStaves(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    protected override string GenerateName() { return "Staves"; }
}
