public class HumanMeat : FoodPile {

    public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Human_Meat;
    
    public HumanMeat() : base(TILE_OBJECT_TYPE.HUMAN_MEAT) {
        AddAdvertisedAction(INTERACTION_TYPE.DISPOSE_FOOD);
    }
    public HumanMeat(SaveDataTileObject saveDataTileObject) : base(saveDataTileObject) { }
    #region Overrides
    public override string ToString() {
        return $"Human Meat {id.ToString()}";
    }
    #endregion
    
}