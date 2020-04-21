using System.Collections.Generic;

public class RackWeapons : TileObject{
    public RackWeapons() {
        Initialize(TILE_OBJECT_TYPE.RACK_WEAPONS);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public RackWeapons(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    
    protected override string GenerateName() { return "Weapon Rack"; }
}
