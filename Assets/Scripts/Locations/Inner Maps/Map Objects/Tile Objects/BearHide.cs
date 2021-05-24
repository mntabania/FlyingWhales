public class BearHide : LeatherPile {
    public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Bear_Hide;
    public BearHide() : base(TILE_OBJECT_TYPE.BEAR_HIDE) { }
    public BearHide(SaveDataTileObject saveDataTileObject) : base(saveDataTileObject) { }
    
    #region Overrides
    public override string ToString() {
        return $"Bear Hide {id.ToString()}";
    }
    #endregion

    
}
