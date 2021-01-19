using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = System.Diagnostics.Debug;

public abstract class Crops : TileObject {
    public enum Growth_State { Growing, Ripe }
    public Growth_State currentGrowthState { get; private set; }
    
    private int _remainingRipeningTicks;
    private int _growthRate; //how fast does this crop grow? (aka how many ticks are subtracted from the remaining ripening ticks per tick)
    private bool hasStartedGrowth;
    
    #region getters
    public int remainingRipeningTicks => _remainingRipeningTicks;
    public override System.Type serializedData => typeof(SaveDataCrops);
    public int growthRate => _growthRate;
    public virtual bool doesNotGrowPerTick => false;
    #endregion
    
    protected Crops() { }
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
            traitContainer.RemoveTrait(this, "Edible");
        } else if (growthState == Growth_State.Ripe) {
            _remainingRipeningTicks = 0;
            StopPerTickGrowth();
            AddAdvertisedAction(INTERACTION_TYPE.HARVEST_PLANT);
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
        if (_remainingRipeningTicks == -1) {
            //if value is set to -1 then it means that this crop has just started growing, set its remaining ripening ticks here
            _remainingRipeningTicks = GetRipeningTicks();
        }
        if (_remainingRipeningTicks == 0) {
            SetGrowthState(Growth_State.Ripe);
        }
        _remainingRipeningTicks = _remainingRipeningTicks - growthRate;
        //Make sure to set this to be at maximum 0, so as not to trigger the above -1 if statement, since that is meant to reset the per tick growth
        _remainingRipeningTicks = Mathf.Max(0, _remainingRipeningTicks);
        if (mapVisual != null) {
            mapVisual.UpdateTileObjectVisual(this);    
        }
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
        Messenger.RemoveListener(Signals.TICK_ENDED, PerTickGrowth);
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        Messenger.AddListener(Signals.TICK_ENDED, PerTickGrowth);
    }
    public override void OnDestroyPOI() {
        base.OnDestroyPOI();
        Messenger.RemoveListener(Signals.TICK_ENDED, PerTickGrowth);
    }

    #region Testing
    public override string GetAdditionalTestingData() {
        string data = base.GetAdditionalTestingData();
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
    
    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        Crops crop = tileObject as Crops;
        Assert.IsNotNull(crop);
        growthState = crop.currentGrowthState;
        remainingRipeningTicks = crop.remainingRipeningTicks;
        growthRate = crop.growthRate;
    }
    public override TileObject Load() {
        TileObject tileObject = base.Load();
        return tileObject;
    }
} 
#endregion
