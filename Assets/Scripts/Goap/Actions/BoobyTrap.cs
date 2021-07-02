using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Traits;

public class BoobyTrap : GoapAction {

    public BoobyTrap() : base(INTERACTION_TYPE.BOOBY_TRAP) {
        actionIconString = GoapActionStateDB.Trap_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Crimes};
    }

    #region Overrides
    public override bool ShouldActionBeAnIntel(ActualGoapNode node) {
        return true;
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Booby Trapped", false, GOAP_EFFECT_TARGET.TARGET));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Trap Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}:";
#endif
        int cost = 10;
#if DEBUG_LOG
        costLog += $" +{cost}(Initial)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return cost;
    }
    public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(actor, target, witness, node, status);
        BoobyTrapped boobyTrapped = target.traitContainer.GetTraitOrStatus<BoobyTrapped>("Booby Trapped");
        boobyTrapped?.AddAwareCharacter(witness);
        return response;
    }
    public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);

        if (target is TileObject tileObject) {
            if (tileObject.IsOwnedBy(witness)) {
                if (witness.traitContainer.HasTrait("Coward")) {
                    reactions.Add(EMOTION.Fear);
                } else {
                    reactions.Add(EMOTION.Anger);
                    reactions.Add(EMOTION.Threatened);
                }
                if (witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.RELATIVE)
                    || witness.relationshipContainer.IsFriendsWith(actor)) {
                    reactions.Add(EMOTION.Betrayal);
                    reactions.Add(EMOTION.Shock);
                }
            } else if (actor.traitContainer.HasTrait("Cultist") && witness.traitContainer.HasTrait("Cultist")) {
                reactions.Add(EMOTION.Approval);
                if (RelationshipManager.IsSexuallyCompatibleOneSided(witness, actor)) {
                    int compatibility = RelationshipManager.Instance.GetCompatibilityBetween(witness, actor);
                    if (UtilityScripts.GameUtilities.RollChance(compatibility * 10)) {
                        reactions.Add(EMOTION.Arousal);
                    }
                }
            } else if (tileObject.characterOwner != null) {
                Character owner = tileObject.characterOwner;
                if (witness.relationshipContainer.HasRelationshipWith(owner, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.RELATIVE)
                    || witness.relationshipContainer.IsFriendsWith(owner)) {
                    if (witness.traitContainer.HasTrait("Coward")) {
                        reactions.Add(EMOTION.Fear);
                    } else {
                        reactions.Add(EMOTION.Shock);
                        reactions.Add(EMOTION.Disapproval);

                        if (witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.RELATIVE)
                            || witness.relationshipContainer.IsFriendsWith(actor)) {
                            reactions.Add(EMOTION.Disappointment);
                        }
                    }
                } else {
                    reactions.Add(EMOTION.Disapproval);

                    if (witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.RELATIVE)
                        || witness.relationshipContainer.IsFriendsWith(actor)) {
                        reactions.Add(EMOTION.Shock);
                        reactions.Add(EMOTION.Disappointment);
                    }
                }
            } else {
                reactions.Add(EMOTION.Disapproval);

                if (witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.RELATIVE)
                    || witness.relationshipContainer.IsFriendsWith(actor)) {
                    reactions.Add(EMOTION.Shock);
                    reactions.Add(EMOTION.Disappointment);
                }
            }
        }
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    public override void OnStopWhileStarted(ActualGoapNode node) {
        base.OnStopWhileStarted(node);
        //Remove added booby trapped action
        node.poiTarget.traitContainer.RemoveTrait(node.poiTarget, "Booby Trapped");
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        //Remove added booby trapped action
        node.poiTarget.traitContainer.RemoveTrait(node.poiTarget, "Booby Trapped");
    }
    public override void OnStoppedInterrupt(ActualGoapNode node) {
        base.OnStoppedInterrupt(node);
        //Remove added booby trapped action
        node.poiTarget.traitContainer.RemoveTrait(node.poiTarget, "Booby Trapped");
    }
    public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        return CRIME_TYPE.Assault;
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
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            return poiTarget.gridTileLocation != null;
        }
        return false;
    }
#endregion
}