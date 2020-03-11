public class HolyIncantation : GoapAction {
    
    public HolyIncantation() : base(INTERACTION_TYPE.HOLY_INCANTATION) {
        actionIconString = GoapActionStateDB.Work_Icon;
        
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FACTION_QUEST_DURATION_DECREASE, conditionKey = "1", isKeyANumber = true, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Incantation Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 1;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            //**Requirements:** Region is Hallowed Ground
            var region = poiTarget.gridTileLocation.parentMap.location.coreTile.region;
            return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null && region.HasTileWithFeature(TileFeatureDB.Hallowed_Ground_Feature);
        }
        return false;
    }
    #endregion

    #region State Effects
    public void AfterIncantationSuccess(ActualGoapNode goapNode) {
        goapNode.actor.faction.activeQuest.AdjustCurrentDuration(GameManager.ticksPerDay);
    }
    #endregion
}