public class IceberryCrop : Crops {
    public override bool doesNotGrowPerTick => true;
    public override TILE_OBJECT_TYPE producedObjectOnHarvest => TILE_OBJECT_TYPE.ICEBERRY;
    
    public IceberryCrop() : base() {
        Initialize(TILE_OBJECT_TYPE.ICEBERRY_CROP);
    }
    public IceberryCrop(SaveDataCrops data) : base(data) { }

    #region Growth State
    public override int GetRipeningTicks() {
        int ticks = GameManager.Instance.GetTicksBasedOnHour(120);
        return ticks;
    }
    #endregion    
}
