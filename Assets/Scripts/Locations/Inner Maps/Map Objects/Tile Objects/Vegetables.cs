public class Vegetables : FoodPile {
    public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Vegetables;
    public Vegetables() : base(TILE_OBJECT_TYPE.VEGETABLES) { }
    public Vegetables(SaveDataTileObject saveDataTileObject) : base(saveDataTileObject) { }
    
    #region Overrides
    public override string ToString() {
        return $"Vegetables {id.ToString()}";
    }
    #endregion
    
}