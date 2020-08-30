public class AnimalMeat : FoodPile {

    public AnimalMeat() : base(TILE_OBJECT_TYPE.ANIMAL_MEAT) { }
    public AnimalMeat(SaveDataTileObject saveDataTileObject) : base(saveDataTileObject) { }

    #region Overrides
    public override string ToString() {
        return $"Animal Meat {id.ToString()}";
    }
    #endregion
    
}
