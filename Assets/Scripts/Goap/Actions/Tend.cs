using Traits;
public class Tend : GoapAction {
    public Tend() : base(INTERACTION_TYPE.TEND) {
        actionIconString = GoapActionStateDB.Harvest_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        shouldAddLogs = false;
        logTags = new[] {LOG_TAG.Work};
    }
    
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Tend Success", goapNode);
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
    public void AfterTendSuccess(ActualGoapNode goapNode) {
        goapNode.target.traitContainer.AddTrait(goapNode.target, "Tended", goapNode.actor);
        goapNode.target.traitContainer.GetTraitOrStatus<Trait>("Tending")?.SetGainedFromDoingAction(goapNode.action.goapType, goapNode.isStealth);
    }
    #endregion
}