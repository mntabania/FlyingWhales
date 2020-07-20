using System.Collections.Generic;
using Traits;
using UnityEngine;
using UtilityScripts;

public class RemoveBuff : GoapAction {
    public RemoveBuff() : base(INTERACTION_TYPE.REMOVE_BUFF) {
        doesNotStopTargetCharacter = true;
        actionIconString = GoapActionStateDB.Stealth_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        isNotificationAnIntel = true;
    }
    
    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Cultist Kit", false, GOAP_EFFECT_TARGET.ACTOR), HasCultistKit);
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Remove Buff Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +0(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 0;
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    #endregion
    
    #region Preconditions
    private bool HasCultistKit(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        return actor.HasItem("Cultist Kit");
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest target, object[] otherData) {
        bool hasMetRequirements = base.AreRequirementsSatisfied(actor, target, otherData);
        if (hasMetRequirements) {
            return target != actor && target.traitContainer.HasTrait("Resting", "Unconscious") && target.traitContainer.HasTraitOf(TRAIT_TYPE.BUFF);
        }
        return false;
    }
    #endregion
    
    #region State Effects
    public void AfterRemoveBuffSuccess(ActualGoapNode goapNode) {
        List<Trait> buffs = goapNode.target.traitContainer.GetAllTraitsOf(TRAIT_TYPE.BUFF);
        Trait randomBuff = CollectionUtilities.GetRandomElement(buffs);
        goapNode.target.traitContainer.RemoveTrait(goapNode.target, randomBuff, goapNode.actor);
        // goapNode.descriptionLog.AddToFillers(null, randomBuff.name, LOG_IDENTIFIER.STRING_1);

        goapNode.actor.UnobtainItem(TILE_OBJECT_TYPE.CULTIST_KIT);
    }
    #endregion

    #region Reactions
    public override string ReactionToActor(Character actor, IPointOfInterest poiTarget, Character witness,
        ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(actor, poiTarget, witness, node, status);
        if (witness.traitContainer.HasTrait("Cultist") == false) {
            //not a cultist
            CrimeManager.Instance.ReactToCrime(witness, actor, node, node.associatedJobType, CRIME_TYPE.SERIOUS);
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status, node);
            if (witness.relationshipContainer.IsFriendsWith(actor) || witness.relationshipContainer.HasOpinionLabelWithCharacter(actor, BaseRelationshipContainer.Acquaintance)) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Despair, witness, actor, status, node); 
            }
            if (witness.traitContainer.HasTrait("Coward")) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, actor, status, node);
            } else if (witness.traitContainer.HasTrait("Psychopath") == false) {
                //witness is not a psychopath
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, actor, status, node);
            }
            if (witness.relationshipContainer.IsEnemiesWith(actor) == false) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
            }
        }
        else {
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, witness, actor, status, node);
            if (RelationshipManager.IsSexuallyCompatible(witness.sexuality, actor.sexuality, witness.gender,
                actor.gender)) {
                int chance = 10 * witness.relationshipContainer.GetCompatibility(actor);
                int roll = Random.Range(0, 100);
                if (roll < chance) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Arousal, witness, actor, status, node);                    
                }
            }
        }
        return response;
    }
    public override string ReactionOfTarget(Character actor, IPointOfInterest poiTarget, ActualGoapNode node,
        REACTION_STATUS status) {
        string response = base.ReactionOfTarget(actor, poiTarget, node, status);
        if (poiTarget is Character targetCharacter) {
            CrimeManager.Instance.ReactToCrime(targetCharacter, actor, node, node.associatedJobType, CRIME_TYPE.SERIOUS);
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, targetCharacter, actor, status, node);
            if (targetCharacter.traitContainer.HasTrait("Coward")) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, targetCharacter, actor, status, node);
            } else {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, targetCharacter, actor, status, node);
            }

            if (targetCharacter.relationshipContainer.IsFriendsWith(actor)) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, targetCharacter, actor, status, node);
            }
        }

        return response;
    }
    #endregion
}