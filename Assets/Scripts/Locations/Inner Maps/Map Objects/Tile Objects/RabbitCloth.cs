public class RabbitCloth : ClothPile {
    public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Rabbit_Cloth;
    public RabbitCloth() : base(TILE_OBJECT_TYPE.RABBIT_CLOTH){ }
    public RabbitCloth(SaveDataTileObject saveDataTileObject) : base(saveDataTileObject) { }
    
    #region Overrides
    public override string ToString() {
        return $"Rabbit Cloth {id.ToString()}";
    }
    #endregion
}
