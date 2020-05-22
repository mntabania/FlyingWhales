public class HumanMeat : FoodPile {

    public HumanMeat() : base(TILE_OBJECT_TYPE.HUMAN_MEAT) { }

    #region Overrides
    public override string ToString() {
        return $"Human Meat {id.ToString()}";
    }
    #endregion
    
}