using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Cry : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.INDIRECT; } }

    private readonly string[] _costTraits = new[] {
        "Worried", "Exhausted", "Traumatized", "Heartbroken", "Betrayed", "Dolorous", "Griefstricken"
    };
    
    public Cry() : base(INTERACTION_TYPE.CRY) {
        actionIconString = GoapActionStateDB.Sad_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Needs};
    }

    #region Overrides
    public override bool ShouldActionBeAnIntel(ActualGoapNode node) {
        return true;
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Cry Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        int cost = UtilityScripts.Utilities.Rng.Next(100, 116);
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}:";
        costLog += $" +{cost.ToString()}(Initial)";
#endif
        int timesCost = 20 * actor.jobComponent.GetNumOfTimesActionDone(this);
        cost += timesCost;
#if DEBUG_LOG
        costLog += $" +{timesCost.ToString()}(10 x Times Cried)";
#endif
        // if (actor.moodComponent.moodState != MOOD_STATE.Bad && actor.moodComponent.moodState != MOOD_STATE.Critical) {
        //     cost += 2000;
        //     costLog += " +2000(not Low and Critical mood)";
        // }
        if (actor.traitContainer.HasTrait(_costTraits)) {
            for (int i = 0; i < _costTraits.Length; i++) {
                string trait = _costTraits[i];
                if (actor.traitContainer.HasTrait(trait)) {
                    int randomAmount = UtilityScripts.Utilities.Rng.Next(10, 31);
                    cost -= randomAmount;
#if DEBUG_LOG
                    costLog += $" -{randomAmount.ToString()}(Has {trait})";       
#endif
                }
            }
        } else {
            cost += 2000;
#if DEBUG_LOG
            costLog += " +2000(does not have any of cost traits)";
#endif
        }
#if DEBUG_LOG
        actor.logComponent.AppendCostLog(costLog);
#endif
        return cost;
    }
    public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
        string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
        if (opinionLabel == RelationshipManager.Enemy || opinionLabel == RelationshipManager.Rival) {
            if (UnityEngine.Random.Range(0, 2) == 0) {
                reactions.Add(EMOTION.Scorn);
            }
        } else if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
            if (!witness.traitContainer.HasTrait("Psychopath")) {
                reactions.Add(EMOTION.Concern);
            }
        } else if (opinionLabel == RelationshipManager.Acquaintance) {
            if (!witness.traitContainer.HasTrait("Psychopath") && UnityEngine.Random.Range(0, 2) == 0) {
                reactions.Add(EMOTION.Concern);
            }
        }
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    //public override void OnStopWhilePerforming(ActualGoapNode node) {
    //    base.OnStopWhilePerforming(node);
    //    if (node.actor.characterClass.IsCombatant()) {
    //        node.actor.needsComponent.AdjustDoNotGetBored(-1);
    //    }
    //}
    public override bool IsHappinessRecoveryAction() {
        return true;
    }
#endregion

#region State Effects
    public void PreCrySuccess(ActualGoapNode goapNode) {
        goapNode.actor.jobComponent.IncreaseNumOfTimesActionDone(this);
        //if (goapNode.actor.characterClass.IsCombatant()) {
        //    goapNode.actor.needsComponent.AdjustDoNotGetBored(1);
        //}
        
        if (goapNode.actor.traitContainer.HasTrait("Griefstricken")) {
            goapNode.descriptionLog.AddToFillers(null, "grieving", LOG_IDENTIFIER.STRING_1);
        } else if (goapNode.actor.traitContainer.HasTrait("Heartbroken")) {
            goapNode.descriptionLog.AddToFillers(null, "feeling heartbroken", LOG_IDENTIFIER.STRING_1);
        } else if (goapNode.actor.traitContainer.HasTrait("Worried")) {
            goapNode.descriptionLog.AddToFillers(null, "worried about someone", LOG_IDENTIFIER.STRING_1);
        } else if (goapNode.actor.traitContainer.HasTrait("Traumatized")) {
            goapNode.descriptionLog.AddToFillers(null, "traumatized", LOG_IDENTIFIER.STRING_1);
        } else if (goapNode.actor.traitContainer.HasTrait("Betrayed")) {
            goapNode.descriptionLog.AddToFillers(null, "feeling betrayed", LOG_IDENTIFIER.STRING_1);
        } else {
            goapNode.descriptionLog.AddToFillers(null, "feeling sad", LOG_IDENTIFIER.STRING_1);
        }
    }
    public void PerTickCrySuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustHappiness(-2f);
    }
    public void AfterCrySuccess(ActualGoapNode goapNode) {
        //Messenger.Broadcast(Signals.CREATE_CHAOS_ORBS, goapNode.actor.marker.transform.position, 
        //    3, goapNode.actor.currentRegion.innerMap);
        //if (goapNode.actor.characterClass.IsCombatant()) {
        //    goapNode.actor.needsComponent.AdjustDoNotGetBored(-1);
        //}
        // goapNode.actor.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, goapNode.actor, "feeling sad");
        //Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, goapNode.actor.marker.transform.position, 2, goapNode.actor.currentRegion.innerMap);
    }
#endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            return actor == poiTarget;
        }
        return false;
    }
#endregion
}