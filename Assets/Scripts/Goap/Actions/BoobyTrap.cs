using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Traits;

public class BoobyTrap : GoapAction {

    public BoobyTrap() : base(INTERACTION_TYPE.BOOBY_TRAP) {
        actionIconString = GoapActionStateDB.Anger_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Booby Trapped", false, GOAP_EFFECT_TARGET.TARGET));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Trap Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}:";
        int cost = 10;
        costLog += $" +{cost}(Initial)";
        actor.logComponent.AppendCostLog(costLog);
        return cost;
    }
    public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness,
        ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(actor, target, witness, node, status);

        BoobyTrapped boobyTrapped = target.traitContainer.GetNormalTrait<BoobyTrapped>("Booby Trapped");
        boobyTrapped?.AddAwareCharacter(witness);
        
        if (target is TileObject tileObject) {
            if (tileObject.IsOwnedBy(witness)) {
                if (witness.traitContainer.HasTrait("Coward")) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, actor, status, node);
                } else {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, actor, status, node);
                }
                if(witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.RELATIVE)
                    || witness.relationshipContainer.IsFriendsWith(actor)) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, witness, actor, status, node);
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status, node);
                }
            } else if (tileObject.characterOwner != null) {
                Character owner = tileObject.characterOwner;
                if (witness.relationshipContainer.HasRelationshipWith(owner, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.RELATIVE)
                    || witness.relationshipContainer.IsFriendsWith(owner)) {
                    if (witness.traitContainer.HasTrait("Coward")) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, actor, status, node);
                    } else {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status, node);
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);

                        if (witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.RELATIVE)
                            || witness.relationshipContainer.IsFriendsWith(actor)) {
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disappointment, witness, actor, status, node);
                        }
                    }
                }
            } else {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);

                if (witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.RELATIVE)
                    || witness.relationshipContainer.IsFriendsWith(actor)) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status, node);
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disappointment, witness, actor, status, node);
                }
            }
        }
        CrimeManager.Instance.ReactToCrime(witness, actor, node, node.associatedJobType, CRIME_TYPE.MISDEMEANOR);
        return response;
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    #endregion

    #region State Effects
    public void PreTrapSuccess(ActualGoapNode goapNode) {
        //NOTE: Booby trapped trait added in pre effect so that anyone that witnesses this action, can access that trait,
        //even if this action has not been finished yet. Booby trap will not be activated by this action, since, booby traps
        //are activated at the start of the action (before this).
        Character actor = goapNode.actor;
        IPointOfInterest target = goapNode.poiTarget;

        Trait trait;
        target.traitContainer.AddTrait(target, "Booby Trapped", out trait, actor);
        if(trait != null) {
            (trait as BoobyTrapped).SetElementType(actor.combatComponent.elementalDamage.type);
        }
    }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return poiTarget.gridTileLocation != null;
        }
        return false;
    }
    #endregion
}