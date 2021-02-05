public class RatMeat : FoodPile {

    public RatMeat() : base(TILE_OBJECT_TYPE.RAT_MEAT) { }
    public RatMeat(SaveDataTileObject saveDataTileObject) : base(saveDataTileObject) { }

    #region Overrides
    public override string ToString() {
        return $"Rat Meat {id.ToString()}";
    }
    #endregion
    
}
