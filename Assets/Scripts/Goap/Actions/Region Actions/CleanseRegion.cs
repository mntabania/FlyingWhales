using Traits;

public class CleanseRegion : GoapAction {
    
    public CleanseRegion() : base(INTERACTION_TYPE.CLEANSE_REGION) {
        actionIconString = GoapActionStateDB.Work_Icon;
        
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_REGION_CORRUPTION, string.Empty, false, GOAP_EFFECT_TARGET.TARGET));
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.CLEAR_REGION_FACTION_OWNER, string.Empty, false, GOAP_EFFECT_TARGET.TARGET));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Cleanse Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 1;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        log.AddToFillers(node.poiTarget.gridTileLocation.parentMap.region.coreTile.region, node.poiTarget.gridTileLocation.parentMap.region.coreTile.region.name, LOG_IDENTIFIER.LANDMARK_1);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            //**Requirements:** Actor has Purifier trait. Region is corrupted. Region does not have a landmark.
            var region = poiTarget.gridTileLocation.parentMap.region.coreTile.region;
            return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null && actor.traitContainer.HasTrait("Purifier")
                   && region.coreTile.isCorrupted && region.mainLandmark.specificLandmarkType == LANDMARK_TYPE.NONE;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreCleanseSuccess(ActualGoapNode goapNode) {
        goapNode.descriptionLog.AddToFillers(goapNode.poiTarget.gridTileLocation.parentMap.region.coreTile.region, goapNode.poiTarget.gridTileLocation.parentMap.region.coreTile.region.name, LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterCleanseSuccess(ActualGoapNode goapNode) {
        var region = goapNode.poiTarget.gridTileLocation.parentMap.region.coreTile.region;
    }
    #endregion
}
