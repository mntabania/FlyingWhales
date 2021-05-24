public class SpiderSilk : ClothPile {
    public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Spider_Silk;
    public SpiderSilk() : base(TILE_OBJECT_TYPE.SPIDER_SILK){ }
    public SpiderSilk(SaveDataTileObject saveDataTileObject) : base(saveDataTileObject) { }
    
    #region Overrides
    public override string ToString() {
        return $"Spider Silk {id.ToString()}";
    }
    #endregion
}
