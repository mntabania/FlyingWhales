using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PoisonVent : TileObject {

    private readonly int _activityCycle;
    private string _currentActivitySchedule;

    #region Getters
    public int activityCycle => _activityCycle;
    public override System.Type serializedData => typeof(SaveDataPoisonVent);
    #endregion
    
    public PoisonVent() {
        Initialize(TILE_OBJECT_TYPE.POISON_VENT);
        _activityCycle = Random.Range(12, 61);
    }
    public PoisonVent(SaveDataPoisonVent data) {
        //SaveDataPoisonVent saveDataPoisonVent = data as SaveDataPoisonVent;
        Assert.IsNotNull(data);
        _activityCycle = data.activityCycle;
    }

    #region Overrides
    public override void ConstructDefaultActions() {
        base.ConstructDefaultActions();
        AddPlayerAction(SPELL_TYPE.ACTIVATE);
        RemovePlayerAction(SPELL_TYPE.SEIZE_OBJECT);
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        ScheduleActivity();
    }
    public override void OnDestroyPOI() {
        base.OnDestroyPOI();
        SchedulingManager.Instance.RemoveSpecificEntry(_currentActivitySchedule);
    }
    public override void ActivateTileObject() {
        ProducePoisonCloud();
    }
    #endregion

    #region Activity Cycle
    private void ScheduleActivity() {
        GameDate dueDate = GameManager.Instance.Today().AddTicks(_activityCycle);
        _currentActivitySchedule = SchedulingManager.Instance.AddEntry(dueDate, ProduceScheduledPoisonCloud, this);
    }
    private void ProduceScheduledPoisonCloud() {
        ProducePoisonCloud();
        ScheduleActivity();
    }
    #endregion

    #region Production
    private void ProducePoisonCloud() {
        PoisonCloud poisonCloud = new PoisonCloud();
        poisonCloud.SetGridTileLocation(gridTileLocation);
        poisonCloud.OnPlacePOI();
        poisonCloud.SetStacks(Random.Range(2, 9));
    }
    #endregion
}

#region Save Data
public class SaveDataPoisonVent : SaveDataTileObject {
    
    public int activityCycle;
    
    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        PoisonVent poisonVent = tileObject as PoisonVent;
        Assert.IsNotNull(poisonVent);
        activityCycle = poisonVent.activityCycle;
    }
}
#endregion