public class MooncrawlerCloth : ClothPile {
    public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Mooncrawler_Cloth;
    public MooncrawlerCloth() : base(TILE_OBJECT_TYPE.MOONCRAWLER_CLOTH){ }
    public MooncrawlerCloth(SaveDataTileObject saveDataTileObject) : base(saveDataTileObject) { }
    
    #region Overrides
    public override string ToString() {
        return $"Mooncrawler Cloth {id.ToString()}";
    }
    #endregion
}
