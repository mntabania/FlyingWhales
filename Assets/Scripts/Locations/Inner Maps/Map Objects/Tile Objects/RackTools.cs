using System.Collections.Generic;

public class RackTools : TileObject{
    public RackTools() {
        Initialize(TILE_OBJECT_TYPE.RACK_TOOLS);
    }
    public RackTools(SaveDataTileObject data) : base(data){
        
    }
    
    protected override string GenerateName() { return "Tools"; }
}
