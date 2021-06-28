public class AnimalMeat : FoodPile {
    public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Animal_Meat;
    public AnimalMeat() : base(TILE_OBJECT_TYPE.ANIMAL_MEAT) { }
    public AnimalMeat(SaveDataTileObject saveDataTileObject) : base(saveDataTileObject) { }

    #region Overrides
    public override string ToString() {
        return $"Animal Meat {id.ToString()}";
    }
    #endregion
    
    #region Eating
    public override void ApplyFoodEffectsToConsumer(Character p_consumer) {
        p_consumer.traitContainer.AddTrait(p_consumer, "Animal Fed");
    }
    #endregion
    
}
