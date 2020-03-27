public class ManaRune : TileObject{
    public ManaRune() {
        Initialize(TILE_OBJECT_TYPE.MANA_RUNE);
    }
    public ManaRune(SaveDataTileObject data) {
        Initialize(data);
    }
}