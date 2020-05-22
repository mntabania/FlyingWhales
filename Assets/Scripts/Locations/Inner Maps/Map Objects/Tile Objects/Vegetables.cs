public class Vegetables : FoodPile {

    public Vegetables() : base(TILE_OBJECT_TYPE.VEGETABLES) { }

    #region Overrides
    public override string ToString() {
        return $"Vegetables {id.ToString()}";
    }
    #endregion
    
}