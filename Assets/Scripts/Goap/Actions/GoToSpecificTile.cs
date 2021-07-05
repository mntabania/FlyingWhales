public class GoToSpecificTile : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.INDIRECT; } }

    public GoToSpecificTile() : base(INTERACTION_TYPE.GO_TO_SPECIFIC_TILE) {
        actionIconString = GoapActionStateDB.No_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        shouldAddLogs = false;
        logTags = new[] { LOG_TAG.Work };
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
        if (node.associatedJobType == JOB_TYPE.GO_TO_WAITING && node.actor.partyComponent.hasParty) {
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