public class ElfMeat : FoodPile {

    public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Elf_Meat;
    
    public ElfMeat() : base(TILE_OBJECT_TYPE.ELF_MEAT) {
        AddAdvertisedAction(INTERACTION_TYPE.DISPOSE_FOOD);
    }
    public ElfMeat(SaveDataTileObject saveDataTileObject) : base(saveDataTileObject) { }

    #region Overrides
    public override string ToString() {
        return $"Elf Meat {id.ToString()}";
    }
    #endregion
    
}