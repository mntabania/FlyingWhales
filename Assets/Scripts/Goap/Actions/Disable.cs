public class Disable : GoapAction {
    public Disable() : base(INTERACTION_TYPE.DISABLE) {
        actionIconString = GoapActionStateDB.Stealth_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.DEMON };
    }
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Disable Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override bool IsInvalidOnVision(ActualGoapNode node) {
        //this action should not be considered invalid if the target is in combat.
        return false;
    }
    #endregion
    
    #region State Effects
    public void AfterDisableSuccess(ActualGoapNode goapNode) {
        goapNode.poiTarget.traitContainer.AddTrait(goapNode.poiTarget, "Ensnared", goapNode.actor);
    }
    #endregion
}