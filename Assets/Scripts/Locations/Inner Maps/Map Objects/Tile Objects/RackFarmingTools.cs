using System.Collections.Generic;

public class RackFarmingTools : TileObject{
    public RackFarmingTools() {
        Initialize(TILE_OBJECT_TYPE.RACK_FARMING_TOOLS);
    }
    public RackFarmingTools(SaveDataTileObject data) : base(data) {
        
    }
    
    protected override string GenerateName() { return "Farming Tools"; }
}
