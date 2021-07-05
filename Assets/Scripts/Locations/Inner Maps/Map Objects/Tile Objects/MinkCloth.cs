public class MinkCloth : ClothPile {
    public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Mink_Cloth;
    public MinkCloth() : base(TILE_OBJECT_TYPE.MINK_CLOTH){ }
    public MinkCloth(SaveDataTileObject saveDataTileObject) : base(saveDataTileObject) { }
    
    #region Overrides
    public override string ToString() {
        return $"Mink Cloth {id.ToString()}";
    }
    #endregion
}
