public class CleanUp : GoapAction {
    public CleanUp() : base(INTERACTION_TYPE.CLEAN_UP) {
        actionIconString = GoapActionStateDB.Clean_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        // shouldAddLogs = false;
        logTags = new[] {LOG_TAG.Work};
    }
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Clean Success", goapNode);
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
    public void AfterCleanSuccess(ActualGoapNode goapNode) {
        goapNode.target.traitContainer.RemoveStatusAndStacks(goapNode.target, "Wet", goapNode.actor);
        goapNode.target.traitContainer.RemoveStatusAndStacks(goapNode.target, "Dirty", goapNode.actor);
    }
    #endregion
}