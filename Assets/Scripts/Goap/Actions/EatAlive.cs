public class EatAlive : GoapAction {
    public EatAlive() : base(INTERACTION_TYPE.EAT_ALIVE) {
        actionIconString = GoapActionStateDB.Eat_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.SPIDER };
    }
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Eat Alive Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    #endregion
    
    #region State Effects
    public void PerTickEatAliveSuccess(ActualGoapNode goapNode) {
        goapNode.poiTarget.AdjustHP(-50, ELEMENTAL_TYPE.Normal, true, goapNode.actor, showHPBar: true);
    }
    #endregion
}