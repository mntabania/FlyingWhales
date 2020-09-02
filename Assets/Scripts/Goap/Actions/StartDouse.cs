using JetBrains.Annotations;

public class StartDouse : GoapAction {
    public StartDouse() : base(INTERACTION_TYPE.START_DOUSE) {
        actionIconString = GoapActionStateDB.Douse_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        shouldAddLogs = false;
    }
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Start Douse Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    #endregion
    
    #region State Effects
    [UsedImplicitly]
    public void PreStartDouseSuccess(ActualGoapNode goapNode) {
        if (goapNode.associatedJob.originalOwner is NPCSettlement settlement) {
            goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Dousing");
            settlement.settlementJobTriggerComponent.OnTakeDouseFireJob(goapNode.actor);
        }
    }
    #endregion
}