public class Phylactery : TileObject {
    
    public Phylactery() {
        Initialize(TILE_OBJECT_TYPE.PHYLACTERY);
    }
    public Phylactery(SaveDataTileObject data) { }
    protected override string GenerateName() { return "Phylactery"; }
}
