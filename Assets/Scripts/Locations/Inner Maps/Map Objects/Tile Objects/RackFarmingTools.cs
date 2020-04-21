using System.Collections.Generic;

public class RackFarmingTools : TileObject{
    public RackFarmingTools() {
        Initialize(TILE_OBJECT_TYPE.RACK_FARMING_TOOLS);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public RackFarmingTools(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    
    protected override string GenerateName() { return "Farming Tools"; }
}
