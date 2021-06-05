public abstract class ClothPile : ResourcePile {
    
    protected ClothPile(TILE_OBJECT_TYPE tileObjectType) : base(RESOURCE.CLOTH) {
        Initialize(tileObjectType, false);
        SetResourceInPile(100);
    }
    protected ClothPile(SaveDataTileObject saveDataTileObject) : base(saveDataTileObject, RESOURCE.CLOTH) { }
    
    #region Overrides
    public override string ToString() {
        return $"Cloth Pile {id.ToString()}";
    }
    #endregion
}
