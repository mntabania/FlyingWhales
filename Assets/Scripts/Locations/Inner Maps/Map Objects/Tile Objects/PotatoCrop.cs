public class PotatoCrop : Crops {
    public override bool doesNotGrowPerTick => true;
    public override TILE_OBJECT_TYPE producedObjectOnHarvest => TILE_OBJECT_TYPE.POTATO;
    
    public PotatoCrop() : base() {
        Initialize(TILE_OBJECT_TYPE.POTATO_CROP);
    }
    public PotatoCrop(SaveDataCrops data) : base(data) { }

    #region Growth State
    public override int GetRipeningTicks() {
        int ticks = GameManager.Instance.GetTicksBasedOnHour(120); //120
        return ticks;
    }
    #endregion
}
