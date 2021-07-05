public class ScaleHide : TileObject {
    public ScaleHide() {
        Initialize(TILE_OBJECT_TYPE.SCALE_HIDE);
    }
    public ScaleHide(SaveDataTileObject saveDataTileObject) : base(saveDataTileObject) { }
    
    #region Overrides
    public override string ToString() {
        return $"Scale Hide {id.ToString()}";
    }
    #endregion
}
