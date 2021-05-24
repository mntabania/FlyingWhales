public class Iceberry : FoodPile {
    
    public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Iceberry;
    
    public Iceberry() : base(TILE_OBJECT_TYPE.ICEBERRY) { }
    public Iceberry(SaveDataTileObject saveDataTileObject) : base(saveDataTileObject) { }
    
    #region Overrides
    public override string ToString() {
        return $"Iceberry {id.ToString()}";
    }
    #endregion
}
