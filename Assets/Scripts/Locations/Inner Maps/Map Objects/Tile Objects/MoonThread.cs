public class MoonThread : ClothPile {
    public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Moon_Thread;
    public MoonThread() : base(TILE_OBJECT_TYPE.MOON_THREAD){ }
    public MoonThread(SaveDataTileObject saveDataTileObject) : base(saveDataTileObject) { }
    
    #region Overrides
    public override string ToString() {
        return $"Moon Thread {id.ToString()}";
    }
    #endregion
}
