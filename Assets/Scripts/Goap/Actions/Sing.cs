using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Sing : GoapAction {

    public Sing() : base(INTERACTION_TYPE.SING) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.MORNING, TIME_IN_WORDS.LUNCH_TIME, TIME_IN_WORDS.AFTERNOON, TIME_IN_WORDS.EARLY_NIGHT, };
        actionIconString = GoapActionStateDB.Entertain_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.ELEMENTAL, RACE.KOBOLD };
        isNotificationAnIntel = true;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Sing Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}:";
        int cost = UtilityScripts.Utilities.rng.Next(90, 131);
        costLog += $" +{cost}(Initial)";
        int numOfTimesActionDone = actor.jobComponent.GetNumOfTimesActionDone(this);
        if (numOfTimesActionDone > 5) {
            cost += 2000;
            costLog += " +2000(Times Played > 5)";
        } else {
            int timesCost = 10 * numOfTimesActionDone;
            cost += timesCost;
            costLog += $" +{timesCost}(10 x Times Played)";
        }
        Trait trait = actor.traitContainer.GetNormalTrait<Trait>("Music Hater", "Music Lover");
        if (trait != null) {
            if (trait.name == "Music Hater") {
                cost += 2000;
                costLog += " +2000(Music Hater)";
            } else {
                cost += -15;
                costLog += " -15(Music Lover)";
            }
        }
        actor.logComponent.AppendCostLog(costLog);
        return cost;
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        actor.needsComponent.AdjustDoNotGetBored(-1);
    }
    public override string ReactionToActor(Character witness, ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(witness, node, status);
        Character actor = node.actor;
        IPointOfInterest target = node.poiTarget;
        Trait trait = witness.traitContainer.GetNormalTrait<Trait>("Music Hater", "Music Lover");
        if (trait != null) {
            if (trait.name == "Music Hater") {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status);
            } else {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, witness, actor, status);
                SEXUALITY sexuality1 = witness.sexuality;
                SEXUALITY sexuality2 = actor.sexuality;
                GENDER gender1 = witness.gender;
                GENDER gender2 = actor.gender;
                if (RelationshipManager.Instance.GetCompatibilityBetween(witness, actor) >= 4
                    && RelationshipManager.IsSexuallyCompatible(sexuality1, sexuality2, gender1, gender2)
                    && witness.moodComponent.moodState != MOOD_STATE.CRITICAL) {
                    int value = 50;
                    if (actor.traitContainer.HasTrait("Ugly")) {
                        value = 20;
                    }
                    if (UnityEngine.Random.Range(0, 100) < value) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Arousal, witness, actor, status);
                    }
                }
            }
        }
        return response;
    }
    #endregion

    #region Effects
    public void PreSingSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustDoNotGetBored(1);
        goapNode.actor.jobComponent.IncreaseNumOfTimesActionDone(this);
        //currentState.SetIntelReaction(SingSuccessIntelReaction);
    }
    public void PerTickSingSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustHappiness(5f);
    }
    public void AfterSingSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustDoNotGetBored(-1);
    }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
                return false;
            }
            return actor == poiTarget && !actor.traitContainer.HasTrait("Music Hater") && (actor.moodComponent.moodState == MOOD_STATE.NORMAL);
        }
        return false;
    }
    #endregion
    //#region Intel Reactions
    //private List<string> SingSuccessIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();

    //    if (status == SHARE_INTEL_STATUS.WITNESSED && recipient.traitContainer.HasTrait("Music Hater") != null) {
    //        recipient.traitContainer.AddTrait(recipient, "Annoyed");
    //        if (recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER) || recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.AFFAIR)) {
    //            if (recipient.CreateBreakupJob(actor) != null) {
    //                Log log = new Log(GameManager.Instance.Today(), "Trait", "MusicHater", "break_up");
    //                log.AddToFillers(recipient, recipient.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //                log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //                log.AddLogToInvolvedObjects();
    //                PlayerManager.Instance.player.ShowNotificationFrom(recipient, log);
    //            }
    //        } else if (!recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
    //            //Otherwise, if the Actor does not yet consider the Target an Enemy, relationship degradation will occur, log:
    //            Log log = new Log(GameManager.Instance.Today(), "Trait", "MusicHater", "degradation");
    //            log.AddToFillers(recipient, recipient.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //            log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //            log.AddLogToInvolvedObjects();
    //            PlayerManager.Instance.player.ShowNotificationFrom(recipient, log);
    //            RelationshipManager.Instance.RelationshipDegradation(actor, recipient);
    //        }
    //    }
    //    return reactions;
    //}
    //#endregion
}

public class SingData : GoapActionData {
    public SingData() : base(INTERACTION_TYPE.SING) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.ELEMENTAL, RACE.KOBOLD };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor == poiTarget && !actor.traitContainer.HasTrait("Music Hater") && (actor.moodComponent.moodState == MOOD_STATE.NORMAL);
    }
}