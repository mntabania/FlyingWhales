using System;
using Traits;
using Random = UnityEngine.Random;

public class Whip : GoapAction {
    public Whip() : base(INTERACTION_TYPE.WHIP) {
        actionIconString = GoapActionStateDB.Work_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
    }
    
    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Injured", false, GOAP_EFFECT_TARGET.TARGET));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Whip Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        if (node.target is Character target && target.traitContainer.HasTrait("Criminal") == false) {
            return REACTABLE_EFFECT.Negative;
        }
        return REACTABLE_EFFECT.Positive;
    }
    public override string ReactionToActor(Character witness, ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(witness, node, status);
        Character target = node.target as Character;
        Criminal criminalTrait = target.traitContainer.GetNormalTrait<Criminal>("Criminal"); 
        if (criminalTrait != null && criminalTrait.crimeData.target == witness) {
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, node.actor, status, node);
        } else {
            if (witness.relationshipContainer.IsFriendsWith(target) 
                && witness.traitContainer.HasTrait("Psychopath") == false) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Resentment, witness, node.actor, status, node);
            }
            if (witness.traitContainer.HasTrait("Psychopath") == false) {
                if ((witness.traitContainer.HasTrait("Coward") && Random.Range(0, 100) < 75) ||
                    (witness.traitContainer.HasTrait("Coward") == false && Random.Range(0, 100) < 15)) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, node.actor, status, node);    
                }
            }
        }
        return response;
    }
    public override string ReactionToTarget(Character witness, ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToTarget(witness, node, status);
        Character target = node.target as Character;
        if (witness.relationshipContainer.HasOpinionLabelWithCharacter(target, BaseRelationshipContainer.Acquaintance)) {
            if (witness.traitContainer.HasTrait("Psychopath") == false && Random.Range(0, 100) < 50) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, target, status, node);
            }
        } else if (witness.relationshipContainer.IsFriendsWith(target)) {
            if (witness.traitContainer.HasTrait("Psychopath") == false) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, target, status, node);
            }
        } else if (witness.relationshipContainer.IsEnemiesWith(target)) {
            if (witness.traitContainer.HasTrait("Diplomatic") == false) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Scorn, witness, target, status, node);
            }
        }
        return response;
    }
    public override string ReactionOfTarget(ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionOfTarget(node, status);
        Character target = node.poiTarget as Character;
        if (Random.Range(0, 100) < 20) {
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Resentment, target, node.actor, status, node);
        }
        if (target.traitContainer.HasTrait("Hothead") || Random.Range(0, 100) < 20) {
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, target, node.actor, status, node);
        }
        return response;
    }
    #endregion

    #region State Effects
    public void AfterWhipSuccess(ActualGoapNode goapNode) {
        Character target = goapNode.target as Character;
        target.traitContainer.RemoveTrait(target, "Criminal", goapNode.actor);
        target.traitContainer.RemoveTrait(target, "Restrained", goapNode.actor);
        target.traitContainer.AddTrait(target, "Injured", goapNode.actor, goapNode);
    }
    #endregion
}