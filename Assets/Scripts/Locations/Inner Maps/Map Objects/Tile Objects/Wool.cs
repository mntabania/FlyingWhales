public class Wool : ClothPile {
    public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Wool;
    
    public Wool() : base(TILE_OBJECT_TYPE.WOOL) { }
    public Wool(SaveDataTileObject saveDataTileObject) : base(saveDataTileObject) { }
    
    #region Overrides
    public override string ToString() {
        return $"Wool {id.ToString()}";
    }
    #endregion    
}
