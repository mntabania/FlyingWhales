using Inner_Maps;

public class Patrol : GoapAction {

    public Patrol() : base(INTERACTION_TYPE.PATROL) {
        actionLocationType = ACTION_LOCATION_TYPE.OVERRIDE;
        actionIconString = GoapActionStateDB.Patrol_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.RATMAN };
        shouldAddLogs = false;
        logTags = new[] {LOG_TAG.Work};
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Patrol Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    public override LocationGridTile GetOverrideTargetTile(ActualGoapNode goapNode) {
        return goapNode.otherData[0].obj as LocationGridTile;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        string stateName = "Target Missing";
        GoapActionInvalidity goapActionInvalidity = new GoapActionInvalidity(false, stateName, "target_unavailable");
        //patrol action should never be invalid
        return goapActionInvalidity;
    }
#endregion
}