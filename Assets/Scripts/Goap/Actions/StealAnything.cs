using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;  
using Traits;
/// <summary>
/// Action used by Kleptomaniacs to steal anything. This bypasses the requirement that the target object should be owned.
/// <see cref="AreRequirementsSatisfied"/>
/// </summary>
public class StealAnything : GoapAction {

    public StealAnything() : base(INTERACTION_TYPE.STEAL_ANYTHING) {
        actionIconString = GoapActionStateDB.Steal_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Crimes};
    }

    #region Overrides
    public override bool ShouldActionBeAnIntel(ActualGoapNode node) {
        return true;
    }
    //Commented this out since we don't want this action to be used when goap planning. This is only for Level 3 Kleptomaniacs that can rob any place.
    // protected override void ConstructBasePreconditionsAndEffects() {
    //     AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_POI, GOAP_EFFECT_TARGET.ACTOR));
    //     AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, GOAP_EFFECT_TARGET.ACTOR));
    //
    // }
    // protected override List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, OtherData[] otherData, out bool isOverridden) {
    //     List<GoapEffect> ee = ObjectPoolManager.Instance.CreateNewExpectedEffectsList();
    //     List<GoapEffect> baseEE = base.GetExpectedEffects(actor, target, otherData, out isOverridden);
    //     if(baseEE != null && baseEE.Count > 0) {
    //         ee.AddRange(baseEE);
    //     }
    //     TileObject item = target as TileObject;
    //     ee.Add(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_POI, conditionKey = item.name, isKeyANumber = false, target = GOAP_EFFECT_TARGET.ACTOR });
    //     if (actor.traitContainer.HasTrait("Kleptomaniac")) {
    //         ee.Add(new GoapEffect(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
    //     }
    //     isOverridden = true;
    //     return ee;
    // }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Steal Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    public override IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode) {
        if (goapNode.poiTarget is TileObject) {
            TileObject item = goapNode.poiTarget as TileObject;
            if (item.isBeingCarriedBy != null) {
                return item.isBeingCarriedBy; //make the actor follow the character that is carrying the item instead.
            }
        }
        return base.GetTargetToGoTo(goapNode);
    }
    public override LocationStructure GetTargetStructure(ActualGoapNode node) {
        IPointOfInterest poiTarget = node.poiTarget;
        TileObject token = poiTarget as TileObject;
        if (token.isBeingCarriedBy != null) {
            return token.isBeingCarriedBy.currentStructure;
        }
        return base.GetTargetStructure(node);
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        string stateName = "Target Missing";
        bool isInvalid = false;
        //steal can never be invalid since requirement handle all cases of invalidity.
        GoapActionInvalidity goapActionInvalidity = new GoapActionInvalidity(isInvalid, stateName);
        return goapActionInvalidity;
    }
    public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
        if (!witness.traitContainer.HasTrait("Cultist")) {
            reactions.Add(EMOTION.Disapproval);
            if (witness.relationshipContainer.IsFriendsWith(actor)) {
                reactions.Add(EMOTION.Disappointment);
                reactions.Add(EMOTION.Shock);
            }
        } else if (witness == target || (target is TileObject tileObject && tileObject.IsOwnedBy(witness))) {
            reactions.Add(EMOTION.Betrayal);
        }
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        return CRIME_TYPE.Theft;
    }
#endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            TileObject item = poiTarget as TileObject;
            if (item.gridTileLocation != null) {
                return true; //return !item.IsOwnedBy(actor);
            } else {
                return item.isBeingCarriedBy != null; // && !item.IsOwnedBy(actor);
            }
        }
        return false;
    }
#endregion

#region State Effects
    //public void PreStealSuccess(ActualGoapNode goapNode) {
    //    //**Note**: This is a Theft crime
    //    //GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
    //    //goapNode.descriptionLog.AddToFillers(goapNode.targetStructure.location, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
    //    //goapNode.descriptionLog.AddToFillers(goapNode.poiTarget as SpecialToken, goapNode.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //}
    public void AfterStealSuccess(ActualGoapNode goapNode) {
        goapNode.actor.PickUpItem(goapNode.poiTarget as TileObject);
        if(goapNode.actor.traitContainer.HasTrait("Kleptomaniac")) {
            goapNode.actor.needsComponent.AdjustHappiness(10);
        }
    }
#endregion
}