using System.Collections.Generic;

public class TableArmor : TileObject{
    public TableArmor() {
        Initialize(TILE_OBJECT_TYPE.TABLE_ARMOR);
    }
    public TableArmor(SaveDataTileObject data) : base(data) {
        
    }
    
    protected override string GenerateName() { return "Armor Table"; }
}
