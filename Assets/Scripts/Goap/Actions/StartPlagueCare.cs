using Traits;
public class StartPlagueCare : GoapAction {
    public StartPlagueCare() : base(INTERACTION_TYPE.START_PLAGUE_CARE) {
        actionIconString = GoapActionStateDB.Work_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        shouldAddLogs = false;
        logTags = new[] {LOG_TAG.Work};
    }
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Care Success", goapNode);
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
    public void AfterCareSuccess(ActualGoapNode goapNode) {
        goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Plague Caring", goapNode.actor);
        goapNode.actor.traitContainer.GetTraitOrStatus<Trait>("Plague Caring")?.SetGainedFromDoingAction(goapNode.action.goapType, goapNode.isStealth);
    }
#endregion
}