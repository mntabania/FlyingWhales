using Locations.Settlements.Settlement_Events;
using System.Collections.Generic;

public class IsPlagued : GoapAction {
    public IsPlagued() : base(INTERACTION_TYPE.IS_PLAGUED) {
        actionIconString = GoapActionStateDB.Sick_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Life_Changes};
    }
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Plague Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 0;
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
        if (witness.relationshipContainer.IsFriendsWith(actor)) {
            reactions.Add(EMOTION.Concern);
        } else if (witness.relationshipContainer.IsEnemiesWith(actor)) {
            reactions.Add(EMOTION.Disgust);
            reactions.Add(EMOTION.Scorn);
        } else {
            reactions.Add(EMOTION.Disgust);
            reactions.Add(EMOTION.Fear);
        }
    }
    public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(actor, target, witness, node, status);
        if (witness.homeSettlement.eventManager.HasActiveEvent(out PlaguedEvent plaguedSettlementEvent) &&
            plaguedSettlementEvent.rulerDecision == PLAGUE_EVENT_RESPONSE.Quarantine && !actor.traitContainer.HasTrait("Quarantined")) {
            witness.homeSettlement.settlementJobTriggerComponent.TriggerQuarantineJob(actor);
        }
        return response;
    }
    public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        return CRIME_TYPE.Plagued;
    }
#endregion

#region State Effects
    public void PrePlagueSuccess(ActualGoapNode goapNode) { }
    public void PerTickPlagueSuccess(ActualGoapNode goapNode) { }
    public void AfterPlagueSuccess(ActualGoapNode goapNode) { }
#endregion
}