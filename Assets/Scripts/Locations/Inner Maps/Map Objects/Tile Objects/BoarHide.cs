public class BoarHide : LeatherPile {
    public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Boar_Hide;
    public BoarHide() : base(TILE_OBJECT_TYPE.BOAR_HIDE){ }
    public BoarHide(SaveDataTileObject saveDataTileObject) : base(saveDataTileObject) { }
    
    #region Overrides
    public override string ToString() {
        return $"Boar Hide {id.ToString()}";
    }
    #endregion
}
