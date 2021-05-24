public class RatMeat : FoodPile {

    public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Rat_Meat;
    
    public RatMeat() : base(TILE_OBJECT_TYPE.RAT_MEAT) { }
    public RatMeat(SaveDataTileObject saveDataTileObject) : base(saveDataTileObject) { }

    #region Overrides
    public override string ToString() {
        return $"Rat Meat {id.ToString()}";
    }
    #endregion
    
}
