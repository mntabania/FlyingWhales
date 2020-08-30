using Inner_Maps;
using UnityEngine.Assertions;

public abstract class Crops : TileObject {
    public enum Growth_State { Growing, Ripe }
    public Growth_State currentGrowthState { get; private set; }
    
    private int _remainingRipeningTicks;
    private int _growthRate; //how fast does this crop grow? (aka how many ticks are subtracted from the remaining ripening ticks per tick)
    private bool hasStartedGrowth;
    
    #region getters
    public int remainingRipeningTicks => _remainingRipeningTicks;
    #endregion
    
    protected Crops() { }
    protected Crops(SaveDataTileObject data) { }

    #region Initialization
    protected override void Initialize(TILE_OBJECT_TYPE tileObjectType, bool shouldAddCommonAdvertisements = true) {
        base.Initialize(tileObjectType, shouldAddCommonAdvertisements);
        SetGrowthRate(1);
        SetGrowthState(Growth_State.Growing);
    }
    #endregion

    #region Growth
    public virtual void SetGrowthState(Growth_State growthState) {
        currentGrowthState = growthState;
        if (growthState == Growth_State.Growing) {
            _remainingRipeningTicks = -1;
            StartPerTickGrowth();
            RemoveAdvertisedAction(INTERACTION_TYPE.HARVEST_PLANT);
            traitContainer.RemoveTrait(this, "Edible");
        } else if (growthState == Growth_State.Ripe) {
            _remainingRipeningTicks = 0;
            StopPerTickGrowth();
            AddAdvertisedAction(INTERACTION_TYPE.HARVEST_PLANT);
            traitContainer.AddTrait(this, "Edible");
        }
        if (mapVisual != null) {
            mapVisual.UpdateTileObjectVisual(this);    
        }
    }
    private void StartPerTickGrowth() {
        if (hasStartedGrowth) { return; }
        hasStartedGrowth = true;
        Messenger.AddListener(Signals.TICK_ENDED, PerTickGrowth);
    }
    private void StopPerTickGrowth() {
        hasStartedGrowth = false;
        Messenger.RemoveListener(Signals.TICK_ENDED, PerTickGrowth);
    }
    
    /// <summary>
    /// function to get how many ticks until this crop becomes ripe.
    /// </summary>
    /// <returns></returns>
    public abstract int GetRipeningTicks();
    private void PerTickGrowth() {
        if (remainingRipeningTicks == -1) {
            //if value is set to -1 then it means that this crop has just started growing, set its remaining ripening ticks here
            _remainingRipeningTicks = GetRipeningTicks();
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
    public void SetRemainingRipeningTicks(int value) {
        _remainingRipeningTicks = value;
    }
    #endregion
    
    public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true,
        bool destroyTileSlots = true) {
        base.OnRemoveTileObject(removedBy, removedFrom, removeTraits, destroyTileSlots);
        Messenger.RemoveListener(Signals.TICK_ENDED, PerTickGrowth);
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        Messenger.AddListener(Signals.TICK_ENDED, PerTickGrowth);
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

#region Save Data
public class SaveDataCrops : SaveDataTileObject {

    public Crops.Growth_State growthState;
    public int remainingRipeningTicks;
    
    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        Crops crop = tileObject as Crops;
        Assert.IsNotNull(crop);
        growthState = crop.currentGrowthState;
        remainingRipeningTicks = crop.remainingRipeningTicks;
    }
    public override TileObject Load() {
        TileObject tileObject = base.Load();
        Crops crops = tileObject as Crops;
        Assert.IsNotNull(crops);
        crops.SetGrowthState(growthState);
        crops.SetRemainingRipeningTicks(remainingRipeningTicks);
        return tileObject;
    }
} 
#endregion
