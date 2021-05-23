public class PineappleCrop : Crops {
    
    public override bool doesNotGrowPerTick => true;
    public override TILE_OBJECT_TYPE producedObjectOnHarvest => TILE_OBJECT_TYPE.PINEAPPLE;
    
    public PineappleCrop() : base() {
        Initialize(TILE_OBJECT_TYPE.PINEAPPLE_CROP);
    }
    public PineappleCrop(SaveDataCrops data) : base(data) { }

    #region Growth State
    public override int GetRipeningTicks() {
        int ticks = GameManager.Instance.GetTicksBasedOnHour(120); //120
        return ticks;
    }
    #endregion    
    
}
