public class Corn : FoodPile {
    
    public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Corn;
    
    public Corn() : base(TILE_OBJECT_TYPE.CORN) { }
    public Corn(SaveDataTileObject saveDataTileObject) : base(saveDataTileObject) { }
    
    #region Overrides
    public override string ToString() {
        return $"Corn {id.ToString()}";
    }
    #endregion
}
