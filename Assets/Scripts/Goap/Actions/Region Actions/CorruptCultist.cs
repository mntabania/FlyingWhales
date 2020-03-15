using Traits;

public class CorruptCultist : GoapAction {
    
    public CorruptCultist() : base(INTERACTION_TYPE.CORRUPT_CULTIST) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
        
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.CHARACTER_TO_MINION, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Corrupt Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 1;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        Cultist cultist = node.actor.traitContainer.GetNormalTrait<Cultist>("Cultist");
        // log.AddToFillers(null, cultist.minionData.className, LOG_IDENTIFIER.STRING_1);
        // log.AddToFillers(node.poiTarget.gridTileLocation.parentMap.location.coreTile.region, node.poiTarget.gridTileLocation.parentMap.location.coreTile.region.name, LOG_IDENTIFIER.LANDMARK_1);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            //**Requirements:** Region has The Profane Landmark Type and Actor has Cultist trait.
            var region = poiTarget.gridTileLocation.parentMap.region.coreTile.region;
            return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null && actor.traitContainer.HasTrait("Cultist")
                   && region.mainLandmark.specificLandmarkType == LANDMARK_TYPE.THE_PROFANE;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreCleanseSuccess(ActualGoapNode goapNode) {
        Cultist cultist = goapNode.actor.traitContainer.GetNormalTrait<Cultist>("Cultist");
        // goapNode.descriptionLog.AddToFillers(null, cultist.minionData.className, LOG_IDENTIFIER.STRING_1);
        // goapNode.descriptionLog.AddToFillers(goapNode.poiTarget.gridTileLocation.parentMap.location.coreTile.region, goapNode.poiTarget.gridTileLocation.parentMap.location.coreTile.region.name, LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterCleanseSuccess(ActualGoapNode goapNode) {
        // Cultist cultist = goapNode.actor.traitContainer.GetNormalTrait<Cultist>("Cultist");
        // goapNode.actor.RecruitAsMinion(cultist.minionData);
    }
    #endregion
}