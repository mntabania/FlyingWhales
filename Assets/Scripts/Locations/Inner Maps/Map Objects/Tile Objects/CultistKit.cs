public class CultistKit : TileObject {
    
    public CultistKit() {
        Initialize(TILE_OBJECT_TYPE.CULTIST_KIT);
    }
    public CultistKit(SaveDataTileObject data) {
        Initialize(data);
    }

    public override string ToString() {
        return $"Cultist Kit {id.ToString()}";
    }
}