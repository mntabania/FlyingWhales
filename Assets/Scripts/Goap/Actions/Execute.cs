using Traits;
using UnityEngine;

public class Execute : GoapAction {
    public Execute() : base(INTERACTION_TYPE.EXECUTE) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
    }
    
    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        // AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Restrained", false, GOAP_EFFECT_TARGET.TARGET), IsTargetRestrained);
        // AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.DEATH, string.Empty, false, GOAP_EFFECT_TARGET.TARGET));
        // AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Criminal", false, GOAP_EFFECT_TARGET.TARGET));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Execute Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness,
        ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(actor, target, witness, node, status);
        Character poiTarget = target as Character;
        Criminal criminalTrait = poiTarget.traitContainer.GetNormalTrait<Criminal>("Criminal"); 
        if (criminalTrait != null && criminalTrait.crimeData.target == witness) {
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, node.actor, status, node);
        } else {
            if (witness.relationshipContainer.IsFriendsWith(poiTarget) 
                && witness.traitContainer.HasTrait("Psychopath") == false) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Resentment, witness, node.actor, status, node);
            }
            if (witness.traitContainer.HasTrait("Psychopath") == false) {
                if (witness.traitContainer.HasTrait("Coward")) {
                    if (Random.Range(0, 100) < 75) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, node.actor, status, node);    
                    } else {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, node.actor, status, node);  
                    }
                } else {
                    if (Random.Range(0, 100) < 15) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, node.actor, status, node);    
                    } else {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, node.actor, status, node);  
                    }
                }
            }
        }
        return response;
    }
    public override string ReactionToTarget(Character actor, IPointOfInterest target, Character witness,
        ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToTarget(actor, target, witness, node, status);
        Character targetCharacter = target as Character;
        if (witness.relationshipContainer.HasOpinionLabelWithCharacter(targetCharacter, BaseRelationshipContainer.Acquaintance)) {
            if (witness.traitContainer.HasTrait("Psychopath") == false) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Sadness, witness, targetCharacter, status, node);
            }
        } else if (witness.relationshipContainer.IsFriendsWith(targetCharacter)) {
            if (witness.traitContainer.HasTrait("Psychopath") == false) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Sadness, witness, targetCharacter, status, node);
            }
        } else if (witness.relationshipContainer.HasOpinionLabelWithCharacter(targetCharacter, BaseRelationshipContainer.Rival)) {
            if (witness.traitContainer.HasTrait("Diplomatic") == false) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Scorn, witness, targetCharacter, status, node);
            }
        }
        return response;
    }
    public override string ReactionOfTarget(Character actor, IPointOfInterest poiTarget, ActualGoapNode node,
        REACTION_STATUS status) {
        string response = base.ReactionOfTarget(actor, poiTarget, node, status);
        Character target = poiTarget as Character;
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
    public void AfterExecuteSuccess(ActualGoapNode goapNode) {
        Character target = goapNode.target as Character;
        target.traitContainer.RemoveTrait(target, "Criminal", goapNode.actor);
        target.traitContainer.RemoveTrait(target, "Restrained", goapNode.actor);
        target.Death("executed", goapNode, goapNode.actor);
    }
    #endregion
    
    #region Preconditions
    private bool IsTargetRestrained(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return poiTarget.traitContainer.HasTrait("Restrained");
    }
    #endregion

}