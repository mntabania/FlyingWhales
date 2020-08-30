using System.Collections.Generic;

public class BedClinic : TileObject{
    public BedClinic() {
        Initialize(TILE_OBJECT_TYPE.BED_CLINIC);
    }
    public BedClinic(SaveDataTileObject data) { }
    
    protected override string GenerateName() { return "Clinic Bed"; }
}
