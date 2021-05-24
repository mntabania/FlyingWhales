public class WolfHide : LeatherPile {
    public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Wolf_Hide;
    public WolfHide() : base(TILE_OBJECT_TYPE.WOLF_HIDE) { }
    public WolfHide(SaveDataTileObject saveDataTileObject) : base(saveDataTileObject) { }
    
    #region Overrides
    public override string ToString() {
        return $"Wolf Hide {id.ToString()}";
    }
    #endregion
}
