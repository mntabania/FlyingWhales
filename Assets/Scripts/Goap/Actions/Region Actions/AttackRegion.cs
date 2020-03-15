public class AttackRegion : GoapAction {
    
    public AttackRegion() : base(INTERACTION_TYPE.ATTACK_REGION) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
        
        advertisedBy = new[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.DESTROY_REGION_LANDMARK, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Attack Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 1;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        log.AddToFillers(node.poiTarget.gridTileLocation.structure.location, node.poiTarget.gridTileLocation.structure.location.name, LOG_IDENTIFIER.LANDMARK_1);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            //**Requirement:** Region has a landmark
            var region = poiTarget.gridTileLocation.parentMap.region.coreTile.region;
            return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null &&
                   region.mainLandmark.specificLandmarkType != LANDMARK_TYPE.NONE && 
                   region.regionTileObject.Advertises(INTERACTION_TYPE.ATTACK_REGION);
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreAttackSuccess(ActualGoapNode goapNode) {
        goapNode.descriptionLog.AddToFillers(goapNode.poiTarget.gridTileLocation.structure.location, goapNode.poiTarget.gridTileLocation.structure.location.name, LOG_IDENTIFIER.LANDMARK_1);
        goapNode.descriptionLog.AddToFillers(goapNode.poiTarget.gridTileLocation.structure.location.coreTile.region, 
            UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetterOnly(goapNode.poiTarget.gridTileLocation.structure.location.coreTile.region.mainLandmark.specificLandmarkType.ToString()),
            LOG_IDENTIFIER.STRING_1);
        goapNode.poiTarget.RemoveAdvertisedAction(INTERACTION_TYPE.ATTACK_REGION); //this is so that other characters cannot attack this region when another character is already attacking it.
    }
    public void AfterAttackSuccess(ActualGoapNode goapNode) {
        goapNode.poiTarget.AddAdvertisedAction(INTERACTION_TYPE.ATTACK_REGION); //return removed advertisement
        LandmarkManager.Instance.CreateNewLandmarkOnTile(goapNode.poiTarget.gridTileLocation.structure.location.coreTile, LANDMARK_TYPE.NONE);
    }
    #endregion
}