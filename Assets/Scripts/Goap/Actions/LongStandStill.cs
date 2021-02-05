public class LongStandStill : GoapAction {

    public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;
    public LongStandStill() : base(INTERACTION_TYPE.LONG_STAND_STILL) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.No_Icon;
        shouldAddLogs = false;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.GOLEM, RACE.DEMON, RACE.WOLF, RACE.ELEMENTAL, RACE.KOBOLD, RACE.MIMIC, RACE.ENT, RACE.RATMAN, RACE.WURM };
        logTags = new[] {LOG_TAG.Needs};
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Stand Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        return 4;
    }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            return actor == poiTarget;
        }
        return false;
    }
    #endregion
}