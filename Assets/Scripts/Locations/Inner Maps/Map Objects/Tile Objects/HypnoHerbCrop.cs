public class HypnoHerbCrop : Crops {
    public override bool doesNotGrowPerTick => true;
    public override TILE_OBJECT_TYPE producedObjectOnHarvest => TILE_OBJECT_TYPE.HYPNO_HERB;
    
    public HypnoHerbCrop() : base() {
        Initialize(TILE_OBJECT_TYPE.HYPNO_HERB_CROP);
    }
    public HypnoHerbCrop(SaveDataCrops data) : base(data) { }

    #region Growth State
    public override int GetRipeningTicks() {
        int ticks = GameManager.Instance.GetTicksBasedOnHour(120);
        return ticks;
    }
    #endregion
}
