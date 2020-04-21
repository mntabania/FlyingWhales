using System.Collections.Generic;

public class RackTools : TileObject{
    public RackTools() {
        Initialize(TILE_OBJECT_TYPE.RACK_TOOLS);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public RackTools(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    
    protected override string GenerateName() { return "Tools"; }
}
