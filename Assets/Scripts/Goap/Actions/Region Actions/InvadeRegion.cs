public class InvadeRegion : GoapAction {
    
    public InvadeRegion() : base(INTERACTION_TYPE.INVADE_REGION) {
        actionIconString = GoapActionStateDB.Work_Icon;
        
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.REGION_OWNED_BY_ACTOR_FACTION, string.Empty, false, GOAP_EFFECT_TARGET.TARGET));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Invade Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 1;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        log.AddToFillers(node.poiTarget.gridTileLocation.parentMap.location.coreTile.region, node.poiTarget.gridTileLocation.parentMap.location.coreTile.region.name, LOG_IDENTIFIER.LANDMARK_1);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            //**Requirements:** Region is owned by Faction different from Actor's Faction. Region is not Corrupted. Region is non-npcSettlement type.
            var region = poiTarget.gridTileLocation.parentMap.location.coreTile.region;
            // return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null && region.owner != null 
            //        && region.owner != actor.faction && region.coreTile.isCorrupted == false 
            //        && region.locationType.IsSettlementType() == false 
            //        && region.regionTileObject.advertisedActions.Contains(this.goapType);
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreInvadeSuccess(ActualGoapNode goapNode) {
        goapNode.poiTarget.RemoveAdvertisedAction(this.goapType);
        goapNode.descriptionLog.AddToFillers(goapNode.poiTarget.gridTileLocation.parentMap.location.coreTile.region, goapNode.poiTarget.gridTileLocation.parentMap.location.coreTile.region.name, LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterInvadeSuccess(ActualGoapNode goapNode) {
        goapNode.poiTarget.AddAdvertisedAction(this.goapType);
        var region = goapNode.poiTarget.gridTileLocation.parentMap.location.coreTile.region;
        region.AddFactionHere(goapNode.actor.faction);
    }
    #endregion
}

