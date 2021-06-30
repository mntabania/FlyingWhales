using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using Debug = System.Diagnostics.Debug;

public abstract class Crops : TileObject {
    public enum Growth_State { Growing, Ripe }
    public Growth_State currentGrowthState { get; private set; }
    
    public int count { set; get; }
    private int _remainingRipeningTicks;
    private int _growthRate; //how fast does this crop grow? (aka how many ticks are subtracted from the remaining ripening ticks per tick)
    private bool hasStartedGrowth;
    
    #region getters
    public int remainingRipeningTicks => _remainingRipeningTicks;
    public override System.Type serializedData => typeof(SaveDataCrops);
    public int growthRate => _growthRate;
    public virtual bool doesNotGrowPerTick => false;
    public abstract TILE_OBJECT_TYPE producedObjectOnHarvest { get; }
    #endregion
    
    protected Crops() { count = 80; }
    protected Crops(SaveDataCrops data) : base(data) { }

    #region Initialization
    protected override void Initialize(TILE_OBJECT_TYPE tileObjectType, bool shouldAddCommonAdvertisements = true) {
        base.Initialize(tileObjectType, shouldAddCommonAdvertisements);
        SetGrowthRate(1);
        SetGrowthState(Growth_State.Growing);
    }
    #endregion

    #region Loading
    public override void LoadSecondWave(SaveDataTileObject data) {
        base.LoadSecondWave(data);
        SaveDataCrops saveDataCrops = data as SaveDataCrops;
        Debug.Assert(saveDataCrops != null, nameof(saveDataCrops) + " != null");
        currentGrowthState = saveDataCrops.growthState;
        SetRemainingRipeningTicks(saveDataCrops.remainingRipeningTicks);
        SetGrowthRate(saveDataCrops.growthRate);
    }
    public override void LoadAdditionalInfo(SaveDataTileObject data) {
        base.LoadAdditionalInfo(data);
        if (mapVisual != null) {
            mapVisual.UpdateTileObjectVisual(this);    
        }
    }
    #endregion

    #region Growth
    public virtual void SetGrowthState(Growth_State growthState) {
        currentGrowthState = growthState;
        if (growthState == Growth_State.Growing) {
            _remainingRipeningTicks = -1;
            StartPerTickGrowth();
            RemoveAdvertisedAction(INTERACTION_TYPE.HARVEST_PLANT);
            RemoveAdvertisedAction(INTERACTION_TYPE.HARVEST_CROPS);
            traitContainer.RemoveTrait(this, "Edible");
        } else if (growthState == Growth_State.Ripe) {
            _remainingRipeningTicks = 0;
            StopPerTickGrowth();
            AddAdvertisedAction(INTERACTION_TYPE.HARVEST_PLANT);
            AddAdvertisedAction(INTERACTION_TYPE.HARVEST_CROPS);
            if (!traitContainer.HasTrait("Edible")) {
                traitContainer.AddTrait(this, "Edible");    
            }
        }
        if (mapVisual != null) {
            mapVisual.UpdateTileObjectVisual(this);    
        }
    }
    private void StartPerTickGrowth() {
        if (doesNotGrowPerTick) { return; }
        if (hasStartedGrowth) { return; }
        hasStartedGrowth = true;
        Messenger.AddListener(Signals.TICK_ENDED, PerTickGrowth);
    }
    private void StopPerTickGrowth() {
        if (doesNotGrowPerTick) { return; }
        hasStartedGrowth = false;
        Messenger.RemoveListener(Signals.TICK_ENDED, PerTickGrowth);
    }
    
    /// <summary>
    /// function to get how many ticks until this crop becomes ripe.
    /// </summary>
    /// <returns></returns>
    public abstract int GetRipeningTicks();
    private void PerTickGrowth() {
#if DEBUG_PROFILER
        Profiler.BeginSample($"{name} - Crop Per Tick Growth");
#endif
        if (_remainingRipeningTicks == -1) {
            //if value is set to -1 then it means that this crop has just started growing, set its remaining ripening ticks here
#if DEBUG_PROFILER
            Profiler.BeginSample($"{name} - Crop Per Tick Growth - Get Ripening");
#endif
            _remainingRipeningTicks = GetRipeningTicks();
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif
        }
        if (_remainingRipeningTicks == 0) {
#if DEBUG_PROFILER
            Profiler.BeginSample($"{name} - Set Growth State");
#endif
            SetGrowthState(Growth_State.Ripe);
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif
        }
        _remainingRipeningTicks = _remainingRipeningTicks - growthRate;
        //Make sure to set this to be at maximum 0, so as not to trigger the above -1 if statement, since that is meant to reset the per tick growth
        _remainingRipeningTicks = Mathf.Max(0, _remainingRipeningTicks);
        if (mapVisual != null) {
#if DEBUG_PROFILER
            Profiler.BeginSample($"{name} - Update Visual");
#endif
            mapVisual.UpdateTileObjectVisual(this);
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif
        }
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
    public void SetGrowthRate(int growthRate) {
        _growthRate = growthRate;
    }
    public void SetRemainingRipeningTicks(int value) {
        _remainingRipeningTicks = value;
    }
#endregion
    
    public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true, bool destroyTileSlots = true) {
        base.OnRemoveTileObject(removedBy, removedFrom, removeTraits, destroyTileSlots);
        //Messenger.RemoveListener(Signals.TICK_ENDED, PerTickGrowth);
        StopPerTickGrowth();
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        StartPerTickGrowth();
        //Messenger.AddListener(Signals.TICK_ENDED, PerTickGrowth);
    }
    public override void OnDestroyPOI() {
        base.OnDestroyPOI();
        StopPerTickGrowth();
        //Messenger.RemoveListener(Signals.TICK_ENDED, PerTickGrowth);
    }

#region Testing
    public override string GetAdditionalTestingData() {
        string data = base.GetAdditionalTestingData();
        data += $" <b>Count:</b> {count.ToString()}";
        data += $"\n\tGrowth State {currentGrowthState.ToString()}";
        data += $"\n\tGrowth Rate {growthRate.ToString()}";
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
    public int growthRate;
    public int count;

    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        Crops crop = tileObject as Crops;
        Assert.IsNotNull(crop);
        growthState = crop.currentGrowthState;
        remainingRipeningTicks = crop.remainingRipeningTicks;
        growthRate = crop.growthRate;
        count = crop.count;
    }
    public override TileObject Load() {
        TileObject tileObject = base.Load();
        (tileObject as Crops).count = count;
        return tileObject;
    }
} 
#endregion
