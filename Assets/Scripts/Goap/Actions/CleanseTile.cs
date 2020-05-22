public class CleanseTile : GoapAction {
    public CleanseTile() : base(INTERACTION_TYPE.CLEANSE_TILE) {
        actionIconString = GoapActionStateDB.Clean_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        shouldAddLogs = false;
    }
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState(goapNode.actor.HasItem(TILE_OBJECT_TYPE.ICE_CRYSTAL) ? "Ice Cleanse Success" : "Cleanse Success",
            goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    #endregion
    
    #region State Effects
    public void AfterIceCleanseSuccess(ActualGoapNode goapNode) {
        goapNode.actor.UnobtainItem(TILE_OBJECT_TYPE.ICE_CRYSTAL);
        goapNode.target.traitContainer.RemoveStatusAndStacks(goapNode.target, "Poisoned", goapNode.actor);
    }
    public void AfterCleanseSuccess(ActualGoapNode goapNode) {
        goapNode.target.traitContainer.RemoveStatusAndStacks(goapNode.target, "Poisoned", goapNode.actor);
    }
    #endregion
}