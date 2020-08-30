public class ElfMeat : FoodPile {

    public ElfMeat() : base(TILE_OBJECT_TYPE.ELF_MEAT) { }
    public ElfMeat(SaveDataTileObject saveDataTileObject) : base(saveDataTileObject) { }

    #region Overrides
    public override string ToString() {
        return $"Elf Meat {id.ToString()}";
    }
    #endregion
    
}