using System.Collections.Generic;
using UnityEngine;

public class PoisonVent : TileObject {

    private readonly int _activityCycle;
    private string _currentActivitySchedule;
    
    public PoisonVent() {
        Initialize(TILE_OBJECT_TYPE.POISON_VENT);
        _activityCycle = Random.Range(12, 61);
    }
    public PoisonVent(SaveDataTileObject data) {
        Initialize(data);
        _activityCycle = Random.Range(12, 61);
    }

    #region Overrides
    public override void ConstructDefaultActions() {
        base.ConstructDefaultActions();
        AddPlayerAction(SPELL_TYPE.ACTIVATE_TILE_OBJECT);
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
        PoisonCloudTileObject poisonCloudTileObject = new PoisonCloudTileObject();
        poisonCloudTileObject.SetGridTileLocation(gridTileLocation);
        poisonCloudTileObject.OnPlacePOI();
        poisonCloudTileObject.SetStacks(Random.Range(2, 9));
    }
    #endregion
}