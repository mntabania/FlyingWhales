using Inner_Maps.Location_Structures;

public class Eyeball : TileObject{
    public Eyeball() {
        Initialize(TILE_OBJECT_TYPE.EYEBALL);
    }
    public Eyeball(SaveDataTileObject data) : base(data) { }

    #region Overrides
    public override bool CanBeSelected() {
        if (gridTileLocation?.structure is DemonicStructure) {
            //do not allow eyes that are part of demonic structure to be selected. This is so that when a tile on the demonic structure is clicked, it will select the structure instead of this.
            return false;
        }
        return true;
    }
    #endregion
}