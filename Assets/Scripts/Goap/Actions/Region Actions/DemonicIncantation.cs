using Traits;

public class DemonicIncantation : GoapAction {
    
    public DemonicIncantation() : base(INTERACTION_TYPE.DEMONIC_INCANTATION) {
        actionIconString = GoapActionStateDB.Work_Icon;
        
        advertisedBy = new[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FACTION_QUEST_DURATION_INCREASE, conditionKey = "1", isKeyANumber = true, target = GOAP_EFFECT_TARGET.ACTOR });
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
            //**Requirements:** Actor is a Cultist. Region is Hallowed Ground.
            var region = poiTarget.gridTileLocation.parentMap.region.coreTile.region;
            return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null && actor.traitContainer.HasTrait("Cultist")
                   && region.HasTileWithFeature(TileFeatureDB.Hallowed_Ground_Feature);
        }
        return false;
    }
    #endregion

    #region State Effects
    public void AfterIncantationSuccess(ActualGoapNode goapNode) {
        goapNode.actor.faction.activeFactionQuest.AdjustCurrentDuration(-GameManager.ticksPerDay);
    }
    #endregion
}
