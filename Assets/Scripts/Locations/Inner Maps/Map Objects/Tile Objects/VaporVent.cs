using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class VaporVent : TileObject {

    private readonly int _activityCycle;
    private string _currentActivitySchedule;
    
    #region Getters
    public int activityCycle => _activityCycle;
    public override System.Type serializedData => typeof(SaveDataVaporVent);
    #endregion
    
    public VaporVent() {
        Initialize(TILE_OBJECT_TYPE.VAPOR_VENT);
        _activityCycle = Random.Range(12, 61);
    }
    public VaporVent(SaveDataVaporVent data) : base(data) {
        //SaveDataVaporVent saveDataVaporVent = data as SaveDataVaporVent;
        Assert.IsNotNull(data);
        _activityCycle = data.activityCycle;
    }

    #region Overrides
    public override void ConstructDefaultActions() {
        base.ConstructDefaultActions();
        // AddPlayerAction(SPELL_TYPE.ACTIVATE);
        RemovePlayerAction(PLAYER_SKILL_TYPE.SEIZE_OBJECT);
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
        Vapor vapor = new Vapor();
        vapor.SetGridTileLocation(gridTileLocation);
        vapor.OnPlacePOI();
        vapor.SetStacks(Random.Range(2, 9));
    }
    #endregion
}

#region Save Data
public class SaveDataVaporVent : SaveDataTileObject {
    
    public int activityCycle;
    
    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        VaporVent vaporVent = tileObject as VaporVent;
        Assert.IsNotNull(vaporVent);
        activityCycle = vaporVent.activityCycle;
    }
}
#endregion