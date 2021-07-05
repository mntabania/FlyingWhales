public class DrinkWater : GoapAction {

    public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.CONSUME;
    public DrinkWater() : base(INTERACTION_TYPE.DRINK_WATER) {
        actionIconString = GoapActionStateDB.Drink_Icon;
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Needs};
    }

    #region Overrides
    // protected override void ConstructBasePreconditionsAndEffects() {
    //     AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
    // }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Drink Success", goapNode);
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
    public void PreDrinkSuccess(ActualGoapNode goapNode) { }
    public void PerTickDrinkSuccess(ActualGoapNode goapNode) { }
    public void AfterDrinkSuccess(ActualGoapNode goapNode) { }
    #endregion
}