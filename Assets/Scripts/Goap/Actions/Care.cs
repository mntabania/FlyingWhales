using Traits;
using UnityEngine;

public class Care : GoapAction {
    public Care() : base(INTERACTION_TYPE.CARE) {
        actionIconString = GoapActionStateDB.FirstAid_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Work};
    }
    
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Care Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
#endregion
    
#region State Effects
    public void PerTickCareSuccess(ActualGoapNode goapNode) {
        Plagued plagued = goapNode.target.traitContainer.GetTraitOrStatus<Plagued>("Plagued");
        if (plagued != null) {
            GameDate originalRemovalDate = goapNode.target.traitContainer.GetLatestExpiryDate(plagued.name);
            if (originalRemovalDate.hasValue) {
                int ticksRemaining = GameManager.Instance.Today().GetTickDifference(originalRemovalDate);
                if (ticksRemaining > GameManager.ticksPerHour) {
                    GameDate newExpiryDate = originalRemovalDate;
                    newExpiryDate.ReduceTicks(GameManager.ticksPerHour * Random.Range(1, 3));
                    if (newExpiryDate.IsBefore(GameManager.Instance.Today())) {
                        //if new expiry date has been set to a tick before this tick, then force it to end on the next tick instead. 
                        newExpiryDate = GameManager.Instance.Today();
                        newExpiryDate.AddTicks(1);
                    }
#if DEBUG_LOG
                    Debug.Log($"{goapNode.target.name} Will reschedule Plagued removal to {newExpiryDate.ToString()} from {originalRemovalDate.ToString()}");
#endif
                    goapNode.target.traitContainer.RescheduleLatestTraitRemoval(goapNode.target, plagued, newExpiryDate);    
                }
            }
        }
    }
    public void AfterCareSuccess(ActualGoapNode goapNode) {
        goapNode.target.traitContainer.AddTrait(goapNode.target, "Plague Cared", goapNode.actor);
        Trait trait = goapNode.actor.traitContainer.GetTraitOrStatus<Trait>("Plague Cared");
        if (trait != null) {
            trait.SetGainedFromDoingAction(goapNode.action.goapType, goapNode.isStealth);
        }
    }
#endregion
}