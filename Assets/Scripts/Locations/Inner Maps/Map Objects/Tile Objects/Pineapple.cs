public class Pineapple : FoodPile {
    
    public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Pineapple;
    
    public Pineapple() : base(TILE_OBJECT_TYPE.PINEAPPLE) { }
    public Pineapple(SaveDataTileObject saveDataTileObject) : base(saveDataTileObject) { }
    
    #region Overrides
    public override string ToString() {
        return $"Pineapple {id.ToString()}";
    }
    #endregion
    
    #region Eating
    public override void ApplyFoodEffectsToConsumer(Character p_consumer) {
        p_consumer.traitContainer.AddTrait(p_consumer, "Pineapple Fed");
    }
    #endregion
}
