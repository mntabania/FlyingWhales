public class DecreaseMood : GoapAction {
    public DecreaseMood() : base(INTERACTION_TYPE.DECREASE_MOOD) {
        actionIconString = GoapActionStateDB.Sad_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.DEMON };
    }
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Decrease Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    #endregion
    
    #region State Effects
    public void AfterDecreaseSuccess(ActualGoapNode goapNode) {
        goapNode.poiTarget.traitContainer.AddTrait(goapNode.poiTarget, "Dolorous", goapNode.actor);
    }
    #endregion
}