public abstract class Crops : TileObject {
    public enum Growth_State { Growing, Ripe }
    public Growth_State currentGrowthState { get; private set; }
    
    private int _remainingRipeningTicks;
    private int _growthRate; //how fast does this crop grow? (aka how many ticks are subtracted from the remaining ripening ticks per tick)

    #region getters
    public int remainingRipeningTicks => _remainingRipeningTicks;
    #endregion
    
    protected Crops() {
        SetGrowthRate(1);
    }
    
    #region Growth
    protected void SetGrowthState(Growth_State growthState) {
        currentGrowthState = growthState;
        if (growthState == Growth_State.Growing) {
            _remainingRipeningTicks = GetRipeningTicks();
            Messenger.AddListener(Signals.TICK_ENDED, PerTickGrowth);
            RemoveAdvertisedAction(INTERACTION_TYPE.HARVEST_PLANT);
        } else if (growthState == Growth_State.Ripe) {
            _remainingRipeningTicks = 0;
            Messenger.RemoveListener(Signals.TICK_ENDED, PerTickGrowth);
            AddAdvertisedAction(INTERACTION_TYPE.HARVEST_PLANT);
        }
        mapVisual.UpdateTileObjectVisual(this);
    }
    
    /// <summary>
    /// function to get how many ticks until this crop becomes ripe.
    /// </summary>
    /// <returns></returns>
    public abstract int GetRipeningTicks();
    private void PerTickGrowth() {
        if (gridTileLocation == null) {
            return;
        }
        if (remainingRipeningTicks <= 0) {
            SetGrowthState(Growth_State.Ripe);
        }
        _remainingRipeningTicks = remainingRipeningTicks - _growthRate;
        mapVisual.UpdateTileObjectVisual(this);
    }
    public void SetGrowthRate(int growthRate) {
        _growthRate = growthRate;
    }
    #endregion
    
    public override void OnDestroyPOI() {
        base.OnDestroyPOI();
        Messenger.RemoveListener(Signals.TICK_ENDED, PerTickGrowth);
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        SetGrowthState(Growth_State.Growing);
    }
    
    #region Testing
    public override string GetAdditionalTestingData() {
        string data = base.GetAdditionalTestingData();
        data += $"\n\tGrowth State {currentGrowthState.ToString()}";
        data += $"\n\tGrowth Rate {_growthRate.ToString()}";
        data += $"\n\tRipening ticks: {GetRipeningTicks().ToString()}";
        data += $"\n\tRemaining ticks until ripe: {remainingRipeningTicks.ToString()}";
        return data;
    }
    #endregion
}
