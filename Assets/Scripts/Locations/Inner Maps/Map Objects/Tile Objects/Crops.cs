public abstract class Crops : TileObject {
    public enum Growth_State { Growing, Ripe }
    
    protected string ripeScheduleKey;
    public Growth_State currentGrowthState { get; private set; }
    private GameDate ripeningDate;

    #region Growth
    protected void SetGrowthState(Growth_State growthState) {
        currentGrowthState = growthState;
        if (growthState == Growth_State.Growing) {
            ScheduleRipening();
            RemoveAdvertisedAction(INTERACTION_TYPE.HARVEST_PLANT);
        } else if (growthState == Growth_State.Ripe) {
            ripeScheduleKey = string.Empty;
            AddAdvertisedAction(INTERACTION_TYPE.HARVEST_PLANT);
        }
        mapVisual.UpdateTileObjectVisual(this);
    }
    private void ScheduleRipening() {
        if (string.IsNullOrEmpty(ripeScheduleKey) == false) { return; } //already have a 
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddTicks(GetRipeningTicks());
        ripeningDate = dueDate;
        ripeScheduleKey =
            SchedulingManager.Instance.AddEntry(dueDate, () => SetGrowthState(Growth_State.Ripe), this);
    }
    /// <summary>
    /// function to get how many ticks until this crop becomes ripe.
    /// </summary>
    /// <returns></returns>
    protected abstract int GetRipeningTicks();
    #endregion

    public override void OnDestroyPOI() {
        base.OnDestroyPOI();
        if (string.IsNullOrEmpty(ripeScheduleKey) == false) {
            SchedulingManager.Instance.RemoveSpecificEntry(ripeScheduleKey);
            ripeScheduleKey = string.Empty;
        }
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        SetGrowthState(Growth_State.Growing);
    }
    
    #region Testing
    public override string GetAdditionalTestingData() {
        string data = base.GetAdditionalTestingData();
        data += $"\n\tGrowth State {currentGrowthState.ToString()}";
        data += $"\n\tRipening ticks: {GetRipeningTicks().ToString()}";
        data += $"\n\tRipening Date: {ripeningDate.ConvertToContinuousDaysWithTime()}";
        return data;
    }
    #endregion
}
