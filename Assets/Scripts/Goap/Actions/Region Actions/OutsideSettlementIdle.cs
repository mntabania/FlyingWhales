public class OutsideSettlementIdle : GoapAction {
    
    public OutsideSettlementIdle() : base(INTERACTION_TYPE.OUTSIDE_SETTLEMENT_IDLE) {
        actionIconString = GoapActionStateDB.No_Icon;
        
        advertisedBy = new[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        shouldAddLogs = false;
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Idle Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 1;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            //**Requirement:** Actor is in a non-npcSettlement region.
            var region = poiTarget.gridTileLocation.parentMap.location.coreTile.region;
            return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null &&
                   region.locationType.IsSettlementType() == false;
        }
        return false;
    }
    #endregion
    
}