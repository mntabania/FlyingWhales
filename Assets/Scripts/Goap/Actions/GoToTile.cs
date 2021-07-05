public class GoToTile : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.INDIRECT; } }

    public GoToTile() : base(INTERACTION_TYPE.GO_TO_TILE) {
        actionIconString = GoapActionStateDB.No_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.UPON_STRUCTURE_ARRIVAL;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        //racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF,
        //    RACE.SPIDER, RACE.DRAGON, RACE.GOLEM, RACE.DEMON, RACE.ELEMENTAL, RACE.KOBOLD, RACE.MIMIC, RACE.ABOMINATION,
        //    RACE.CHICKEN, RACE.SHEEP, RACE.PIG, RACE.NYMPH, RACE.WISP, RACE.SLUDGE, RACE.GHOST, RACE.LESSER_DEMON, RACE.ANGEL };
        shouldAddLogs = false;
        logTags = new[] {LOG_TAG.Work};
    }
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Go Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    public override void OnMoveToDoAction(ActualGoapNode node) {
        base.OnMoveToDoAction(node);
        if(node.associatedJobType == JOB_TYPE.GO_TO_WAITING && node.actor.partyComponent.hasParty) {
            node.actor.partyComponent.currentParty.AddMemberThatJoinedQuest(node.actor);
        }
    }
    #endregion

    public void AfterGoSuccess(ActualGoapNode goapNode) {
        if (goapNode.associatedJobType == JOB_TYPE.GO_TO_WAITING && goapNode.actor.partyComponent.hasParty) {
            goapNode.actor.partyComponent.currentParty.AddMemberThatJoinedQuest(goapNode.actor);
        }
    }
}