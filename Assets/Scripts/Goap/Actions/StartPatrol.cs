using JetBrains.Annotations;

public class StartPatrol : GoapAction {
    public StartPatrol() : base(INTERACTION_TYPE.START_PATROL) {
        actionIconString = GoapActionStateDB.Patrol_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        shouldAddLogs = false;
    }
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Start Patrol Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    #endregion
    
    #region State Effects
    [UsedImplicitly]
    public void AfterStartPatrolSuccess(ActualGoapNode goapNode) {
        goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Patrolling");
    }
    #endregion
}