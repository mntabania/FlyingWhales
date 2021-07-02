using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;  
using Traits;

public class StealCoins : GoapAction {

    public StealCoins() : base(INTERACTION_TYPE.STEAL_COINS) {
        actionIconString = GoapActionStateDB.Steal_Icon;
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Crimes};
    }

    #region Overrides
    public override bool ShouldActionBeAnIntel(ActualGoapNode node) {
        return true;
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_POI, GOAP_EFFECT_TARGET.ACTOR));
        AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, GOAP_EFFECT_TARGET.ACTOR));

    }
    protected override List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, OtherData[] otherData, out bool isOverridden) {
        List<GoapEffect> ee = ObjectPoolManager.Instance.CreateNewExpectedEffectsList();
        List<GoapEffect> baseEE = base.GetExpectedEffects(actor, target, otherData, out isOverridden);
        if(baseEE != null && baseEE.Count > 0) {
            ee.AddRange(baseEE);
        }
        if (actor.traitContainer.HasTrait("Kleptomaniac")) {
            ee.Add(new GoapEffect(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
        }
        isOverridden = true;
        return ee;
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Steal Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}:";
#endif
        if (actor.traitContainer.HasTrait("Enslaved")) {
            if (target.gridTileLocation == null || !target.gridTileLocation.IsInHomeOf(actor)) {
#if DEBUG_LOG
                costLog += $" +2000(Slave, target is not in actor's home)";
                actor.logComponent.AppendCostLog(costLog);
#endif
                return 2000;
            }
        }
        int cost = UtilityScripts.Utilities.Rng.Next(300, 351);
#if DEBUG_LOG
        costLog += $" +{cost}(Initial)";
#endif
        if (actor.traitContainer.HasTrait("Kleptomaniac")) {
            cost = UtilityScripts.Utilities.Rng.Next(90, 151);
#if DEBUG_LOG
            costLog = " {cost}(Kleptomaniac)";
#endif
        } else {
            if(target is Character targetCharacter) {
                string opinionLabel = actor.relationshipContainer.GetOpinionLabel(targetCharacter);
                if (actor.moodComponent.moodState == MOOD_STATE.Normal || opinionLabel == RelationshipManager.Acquaintance ||
                   opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                    cost += 2000;
#if DEBUG_LOG
                    costLog += " +2000(not Kleptomaniac, Friend/Close/Acquaintance)";
#endif
                } else if (actor.moodComponent.moodState == MOOD_STATE.Bad) {
                    cost += UtilityScripts.Utilities.Rng.Next(500, 601);
                } else if (actor.moodComponent.moodState == MOOD_STATE.Critical) {
                    cost += UtilityScripts.Utilities.Rng.Next(120, 201);
                }
            }
        }
#if DEBUG_LOG
        actor.logComponent.AppendCostLog(costLog);
#endif
        return cost;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        string stateName = "Target Missing";
        bool isInvalid = false;
        //pickpocket can never be invalid since requirement handle all cases of invalidity.
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
    public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
        if (target is Character targetCharacter) {
            reactions.Add(EMOTION.Disappointment);
            if (targetCharacter.traitContainer.HasTrait("Hothead") || UnityEngine.Random.Range(0, 100) < 35) {
                reactions.Add(EMOTION.Anger);
            }
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
            Character targetCharacter = poiTarget as Character;
            if(actor != targetCharacter) {
                return targetCharacter.moneyComponent.HasCoins();
            }
        }
        return false;
    }
#endregion

#region State Effects
    public void AfterStealSuccess(ActualGoapNode goapNode) {
        int coins = 0;
        if (goapNode.poiTarget is Character target) {
            coins = target.moneyComponent.coins;
            target.moneyComponent.AdjustCoins(-coins);
        }
        goapNode.actor.moneyComponent.AdjustCoins(coins);
        if (goapNode.actor.traitContainer.HasTrait("Kleptomaniac")) {
            goapNode.actor.needsComponent.AdjustHappiness(10);
        }
    }
#endregion
}