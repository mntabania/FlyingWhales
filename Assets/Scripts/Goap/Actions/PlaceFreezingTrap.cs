public class PlaceFreezingTrap : GoapAction {
    public PlaceFreezingTrap() : base(INTERACTION_TYPE.PLACE_FREEZING_TRAP) {
        actionIconString = GoapActionStateDB.Work_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.KOBOLD };
        logTags = new[] {LOG_TAG.Work};
    }
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Place Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
#endregion
    
#region State Effects
    public void AfterPlaceSuccess(ActualGoapNode goapNode) {
        if (goapNode.poiTarget is GenericTileObject genericTileObject) {
            genericTileObject.gridTileLocation.tileObjectComponent.SetHasFreezingTrap(true, false, RACE.KOBOLD);
        }
    }
#endregion
}