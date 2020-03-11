public class SnuffTornado : GoapAction {

    public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.INDIRECT;
    
    public SnuffTornado() : base(INTERACTION_TYPE.SNUFF_TORNADO) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
        
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.ELEMENTAL, RACE.KOBOLD };
        actionLocationType = ACTION_LOCATION_TYPE.TARGET_IN_VISION;
    }


    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
//        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = string.Empty, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Snuff Success", goapNode);
           
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void AfterSnuffSuccess(ActualGoapNode goapNode) {
        TornadoTileObject tornado = goapNode.poiTarget as TornadoTileObject;
        tornado.ForceExpire();
    }
    #endregion
}
