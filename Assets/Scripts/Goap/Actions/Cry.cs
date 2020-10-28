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
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        isNotificationAnIntel = true;
        logTags = new[] {LOG_TAG.Needs};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Cry Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}:";
        int cost = UtilityScripts.Utilities.Rng.Next(100, 116);
        costLog += $" +{cost.ToString()}(Initial)";
        int timesCost = 20 * actor.jobComponent.GetNumOfTimesActionDone(this);
        cost += timesCost;
        costLog += $" +{timesCost.ToString()}(10 x Times Cried)";
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
                    costLog += $" -{randomAmount.ToString()}(Has {trait})";            
                }
            }
        } else {
            cost += 2000;
            costLog += " +2000(does not have any of cost traits)";    
        }
        actor.logComponent.AppendCostLog(costLog);
        return cost;
    }
    public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness,
        ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(actor, target, witness, node, status);
        string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
        if (opinionLabel == RelationshipManager.Enemy || opinionLabel == RelationshipManager.Rival) {
            if (UnityEngine.Random.Range(0, 2) == 0) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Scorn, witness, actor, status, node);
            }
        } else if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
            if (!witness.traitContainer.HasTrait("Psychopath")) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor, status, node);
            }        
        } else if (opinionLabel == RelationshipManager.Acquaintance) {
            if (!witness.traitContainer.HasTrait("Psychopath") && UnityEngine.Random.Range(0, 2) == 0) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor, status, node);
            }        
        } 
        return response;
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        if (node.actor.characterClass.IsCombatant()) {
            node.actor.needsComponent.AdjustDoNotGetBored(-1);
        }
    }
    #endregion    

    #region State Effects
    public void PreCrySuccess(ActualGoapNode goapNode) {
        goapNode.actor.jobComponent.IncreaseNumOfTimesActionDone(this);
        if (goapNode.actor.characterClass.IsCombatant()) {
            goapNode.actor.needsComponent.AdjustDoNotGetBored(1);
        }
    }
    public void PerTickCrySuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustHappiness(-3.33f);
    }
    public void AfterCrySuccess(ActualGoapNode goapNode) {
        //Messenger.Broadcast(Signals.CREATE_CHAOS_ORBS, goapNode.actor.marker.transform.position, 
        //    3, goapNode.actor.currentRegion.innerMap);
        if (goapNode.actor.characterClass.IsCombatant()) {
            goapNode.actor.needsComponent.AdjustDoNotGetBored(-1);
        }
        goapNode.actor.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, goapNode.actor, "feeling sad");
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return actor == poiTarget;
        }
        return false;
    }
    #endregion
}