public class Neutralize : GoapAction {

    public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.INDIRECT;
    
    public Neutralize() : base(INTERACTION_TYPE.NEUTRALIZE) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        actionLocationType = ACTION_LOCATION_TYPE.TARGET_IN_VISION;
        logTags = new[] {LOG_TAG.Work, LOG_TAG.Major};
    }


    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
//        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = string.Empty, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Neutralize Success", goapNode);
           
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
#endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (poiTarget is MovingTileObject movingTileObject && movingTileObject.hasExpired) {
                return false;
            }
            return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
        }
        return false;
    }
#endregion

#region State Effects
    public void AfterNeutralizeSuccess(ActualGoapNode goapNode) {
        TileObject dangerousTileObject = goapNode.poiTarget as TileObject;
        if (dangerousTileObject is PoisonCloud poisonCloud) {
            poisonCloud.SetDoExpireEffect(false);
        }
        dangerousTileObject.Neutralize();
    }
#endregion
}
