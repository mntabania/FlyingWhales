using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Sing : GoapAction {

    public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.VERBAL;

    public Sing() : base(INTERACTION_TYPE.SING) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        //validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.MORNING, TIME_IN_WORDS.AFTERNOON, TIME_IN_WORDS.EARLY_NIGHT, };
        actionIconString = GoapActionStateDB.Sing_Icon;
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
        SetState("Sing Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}:";
#endif
        int cost = UtilityScripts.Utilities.Rng.Next(85, 126);
#if DEBUG_LOG
        costLog += $" +{cost}(Initial)";
#endif
        int numOfTimesActionDone = actor.jobComponent.GetNumOfTimesActionDone(this);
        if (numOfTimesActionDone > 5) {
            cost += 2000;
#if DEBUG_LOG
            costLog += " +2000(Times Played > 5)";
#endif
        }

        if (actor.traitContainer.HasTrait("Music Hater") || !actor.limiterComponent.isSociable || actor.marker.HasEnemyOrRivalInVision()) {
            cost += 2000;
#if DEBUG_LOG
            costLog += " +2000 (Actor is Music Hater or is Unsociable or has Enemy/Rival in vision)";
#endif
        }
        if (actor.traitContainer.HasTrait("Music Lover")) {
            cost -= 20;
#if DEBUG_LOG
            costLog += " -20 (Actor is Music Lover)";
#endif
        }
        int timesCost = 10 * numOfTimesActionDone;
        cost += timesCost;
#if DEBUG_LOG
        costLog += $" +{timesCost}(10 x Times Played)";

        actor.logComponent.AppendCostLog(costLog);
#endif
        return cost;
    }
    //public override void OnStopWhilePerforming(ActualGoapNode node) {
    //    base.OnStopWhilePerforming(node);
    //    Character actor = node.actor;
    //    actor.needsComponent.AdjustDoNotGetBored(-1);
    //}
    public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
        Trait trait = witness.traitContainer.GetTraitOrStatus<Trait>("Music Hater", "Music Lover");
        if (trait != null) {
            if (trait.name == "Music Hater") {
                reactions.Add(EMOTION.Disapproval);
                if (witness.HasAfflictedByPlayerWith(trait)) {
                    PLAYER_SKILL_TYPE playerSkillType = trait.GetAfflictionSkillType();
                    if (playerSkillType != PLAYER_SKILL_TYPE.NONE) {
                        PlayerSkillData playerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(playerSkillType);
                        SkillData skillData = PlayerSkillManager.Instance.GetSkillData(playerSkillType);
                        if (playerSkillData.afflictionUpgradeData.HasAddedBehaviourForLevel(AFFLICTION_SPECIFIC_BEHAVIOUR.Angry_Upon_Hear_Music, skillData.currentLevel)) {
                            reactions.Add(EMOTION.Anger);
                        }
                    }
                }
            } else {
                reactions.Add(EMOTION.Approval);
                if (RelationshipManager.Instance.GetCompatibilityBetween(witness, actor) >= 4 &&
                    RelationshipManager.IsSexuallyCompatible(witness, actor) && witness.moodComponent.moodState != MOOD_STATE.Critical) {
                    int value = 50;
                    if (actor.traitContainer.HasTrait("Unattractive")) {
                        value = 20;
                    }
                    if (UnityEngine.Random.Range(0, 100) < value) {
                        reactions.Add(EMOTION.Arousal);
                    }
                }
            }
        }
    }
    public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        MusicHater musicHater = witness.traitContainer.GetTraitOrStatus<MusicHater>("Music Hater");
        musicHater?.ReactToMusicPerformer(witness, actor);
        return base.ReactionToActor(actor, target, witness, node, status);
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        if (witness.traitContainer.HasTrait("Music Hater")) {
            return REACTABLE_EFFECT.Negative;
        } else if (witness.traitContainer.HasTrait("Music Lover")) {
            return REACTABLE_EFFECT.Positive;
        }
        return REACTABLE_EFFECT.Neutral;
    }
    public override bool IsHappinessRecoveryAction() {
        return true;
    }
#endregion

#region Effects
    public void PreSingSuccess(ActualGoapNode goapNode) {
        //goapNode.actor.needsComponent.AdjustDoNotGetBored(1);
        goapNode.actor.jobComponent.IncreaseNumOfTimesActionDone(this);
        //currentState.SetIntelReaction(SingSuccessIntelReaction);
    }
    public void PerTickSingSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustHappiness(6f);
    }
    //public void AfterSingSuccess(ActualGoapNode goapNode) {
    //    goapNode.actor.needsComponent.AdjustDoNotGetBored(-1);
    //}
#endregion

#region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapStructureIsNot(poiTarget.gridTileLocation.structure)) {
                return false;
            }
            if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapAreaIsNot(poiTarget.gridTileLocation.area)) {
                return false;
            }
            return actor == poiTarget && !actor.traitContainer.HasTrait("Music Hater") && (actor.moodComponent.moodState == MOOD_STATE.Normal);
        }
        return false;
    }
#endregion
}