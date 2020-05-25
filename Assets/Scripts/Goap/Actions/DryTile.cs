﻿public class DryTile : GoapAction {
    public DryTile() : base(INTERACTION_TYPE.DRY_TILE) {
        actionIconString = GoapActionStateDB.Clean_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        shouldAddLogs = false;
    }
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Dry Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    #endregion
    
    #region State Effects
    public void AfterDrySuccess(ActualGoapNode goapNode) {
        goapNode.target.traitContainer.RemoveStatusAndStacks(goapNode.target, "Wet", goapNode.actor);
    }
    #endregion
}