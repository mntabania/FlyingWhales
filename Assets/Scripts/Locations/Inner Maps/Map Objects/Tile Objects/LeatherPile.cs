public abstract class LeatherPile : ResourcePile {
    
    protected LeatherPile(TILE_OBJECT_TYPE tileObjectType) : base(RESOURCE.LEATHER) {
        Initialize(tileObjectType, false);
        SetResourceInPile(100);
    }
    protected LeatherPile(SaveDataTileObject saveDataTileObject) : base(saveDataTileObject, RESOURCE.CLOTH) { }
    
    #region Overrides
    public override string ToString() {
        return $"Leather Pile {id.ToString()}";
    }
    #endregion
}
