using Inner_Maps;

public class Patrol : GoapAction {

    public Patrol() : base(INTERACTION_TYPE.PATROL) {
        actionLocationType = ACTION_LOCATION_TYPE.OVERRIDE;
        actionIconString = GoapActionStateDB.Patrol_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON };
        shouldAddLogs = false;
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Patrol Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override LocationGridTile GetOverrideTargetTile(ActualGoapNode goapNode) {
        return goapNode.otherData[0] as LocationGridTile;
    }
    #endregion
}