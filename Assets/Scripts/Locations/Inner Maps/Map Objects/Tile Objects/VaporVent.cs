using System.Collections.Generic;
using UnityEngine;

public class VaporVent : TileObject {

    private readonly int _activityCycle;
    private string _currentActivitySchedule;
    
    public VaporVent() {
        Initialize(TILE_OBJECT_TYPE.VAPOR_VENT);
        _activityCycle = Random.Range(12, 61);
    }
    public VaporVent(SaveDataTileObject data) {
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
        ProduceVapor();
    }
    #endregion
    
    
    #region Activity Cycle
    private void ScheduleActivity() {
        GameDate dueDate = GameManager.Instance.Today().AddTicks(_activityCycle);
        _currentActivitySchedule = SchedulingManager.Instance.AddEntry(dueDate, ProduceScheduledVapor, this);
    }
    private void ProduceScheduledVapor() {
        ProduceVapor();
        ScheduleActivity();
    }
    #endregion

    #region Production
    private void ProduceVapor() {
        VaporTileObject vaporTileObject = new VaporTileObject();
        vaporTileObject.SetGridTileLocation(gridTileLocation);
        vaporTileObject.OnPlacePOI();
        vaporTileObject.SetStacks(Random.Range(2, 9));
    }
    #endregion

   
}