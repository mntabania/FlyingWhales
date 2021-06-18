using JetBrains.Annotations;

public class StartDry : GoapAction {
    public StartDry() : base(INTERACTION_TYPE.START_DRY) {
        actionIconString = GoapActionStateDB.Clean_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        shouldAddLogs = false;
        logTags = new[] {LOG_TAG.Work};
    }
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Start Dry Success", goapNode);
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
    [UsedImplicitly]
    public void PreStartDrySuccess(ActualGoapNode goapNode) {
        // if (goapNode.associatedJob.originalOwner is NPCSettlement settlement) {
        //     goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Drying");
        //     settlement.settlementJobTriggerComponent.OnTakeDryTileJob(goapNode.actor);
        // }
    }
#endregion
}