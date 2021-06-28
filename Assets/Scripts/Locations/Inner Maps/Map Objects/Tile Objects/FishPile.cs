public class FishPile : FoodPile {
    
    public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Fish;

    public FishPile() : base(TILE_OBJECT_TYPE.FISH_PILE) { }
    public FishPile(SaveDataTileObject saveDataTileObject) : base(saveDataTileObject) { }

    #region Overrides
    public override string ToString() {
        return $"Fish Pile {id.ToString()}";
    }
    #endregion
    
    #region Eating
    public override void ApplyFoodEffectsToConsumer(Character p_consumer) {
        p_consumer.traitContainer.AddTrait(p_consumer, "Fish Fed");
    }
    #endregion
}