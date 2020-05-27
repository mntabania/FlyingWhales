public class PlaceFreezingTrap : GoapAction {
    public PlaceFreezingTrap() : base(INTERACTION_TYPE.PLACE_FREEZING_TRAP) {
        actionIconString = GoapActionStateDB.Work_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.KOBOLD };
    }
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Place Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    #endregion
    
    #region State Effects
    public void AfterPlaceSuccess(ActualGoapNode goapNode) {
        if (goapNode.poiTarget is GenericTileObject genericTileObject) {
            TrapChecker freezingTrapChecker = goapNode.otherData[0] as TrapChecker;
            genericTileObject.gridTileLocation.SetHasFreezingTrap(true, freezingTrapChecker);
        }
    }
    #endregion
}