using System;
using System.Collections;
using System.Collections.Generic;
using Goap.Unique_Action_Data;
using UnityEngine;  
using Traits;
using UtilityScripts;

public class FeedSelf : GoapAction {

    public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;
    //public override Type uniqueActionDataType => typeof(FeedUAD);
    public FeedSelf() : base(INTERACTION_TYPE.FEED_SELF) {
        actionIconString = GoapActionStateDB.Happy_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        //racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        //isNotificationAnIntel = true;
        logTags = new[] {LOG_TAG.Needs};
    }

    #region Overrides
    //protected override void ConstructBasePreconditionsAndEffects() {
    //    AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TAKE_POI, conditionKey = "Food Pile", isKeyANumber = false, target = GOAP_EFFECT_TARGET.ACTOR }, ActorHasFood);
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.TARGET });
    //}
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Feed Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    //public override void OnStopWhileStarted(ActualGoapNode node) {
    //    base.OnStopWhileStarted(node);
    //    Character actor = node.actor;
    //    actor.UncarryPOI();
    //}
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        //actor.UncarryPOI();
        poiTarget.traitContainer.RemoveTrait(poiTarget, "Eating");
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            if ((poiTarget as Character).carryComponent.IsNotBeingCarried() == false) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.reason = "target_carried";
            }
        }
        return goapActionInvalidity;
    }
    //public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
    //    string response = base.ReactionToActor(actor, target, witness, node, status);
    //    if(target is Character targetCharacter) {
    //        FeedUAD uniqueData = node.GetConvertedUniqueActionData<FeedUAD>();
    //        string opinionOfTarget = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
    //        if (uniqueData.usedPoisonedFood) {
    //            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status, node);
    //            if (opinionOfTarget == RelationshipManager.Friend || opinionOfTarget == RelationshipManager.Close_Friend) {
    //                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);    
    //            }
    //        } else {
    //            if (opinionOfTarget == RelationshipManager.Friend || opinionOfTarget == RelationshipManager.Close_Friend) {
    //                if (!witness.traitContainer.HasTrait("Psychopath")) {
    //                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Gratefulness, witness, actor, status, node);
    //                }
    //            } else if (opinionOfTarget == RelationshipManager.Rival) {
    //                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
    //            }    
    //        }
    //    }
    //    return response;
    //}
    //public override string ReactionOfTarget(Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status) {
    //    string response = base.ReactionOfTarget(actor, target, node, status);
    //    if (target is Character targetCharacter) {
    //        FeedUAD uniqueData = node.GetConvertedUniqueActionData<FeedUAD>();
    //        if (uniqueData.usedPoisonedFood) {
    //            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, targetCharacter, actor, status, node);
    //            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, targetCharacter, actor, status, node);
    //        } else {
    //            if (!targetCharacter.traitContainer.HasTrait("Psychopath")) {
    //                if (targetCharacter.relationshipContainer.IsEnemiesWith(actor)) {
    //                    if (UnityEngine.Random.Range(0, 100) < 30) {
    //                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Gratefulness, targetCharacter, actor, status, node);
    //                    }
    //                    if (UnityEngine.Random.Range(0, 100) < 20) {
    //                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Embarassment, targetCharacter, actor, status, node);
    //                    }
    //                } else {
    //                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Gratefulness, targetCharacter, actor, status, node);
    //                }
    //            }    
    //        }

    //    }
    //    return response;
    //}
    //public override void OnActionStarted(ActualGoapNode node) {
    //    base.OnActionStarted(node);
    //    for (int i = 0; i < node.actor.items.Count; i++) {
    //        if(node.actor.items[i].HasResourceAmount(RESOURCE.FOOD, 12)) {
    //            node.actor.ShowItemVisualCarryingPOI(node.actor.items[i]);
    //            break;
    //        }
    //    }
    //}
    //public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
    //    FeedUAD uniqueData = node.GetConvertedUniqueActionData<FeedUAD>();
    //    if (uniqueData.usedPoisonedFood) {
    //        return REACTABLE_EFFECT.Negative;
    //    }
    //    return REACTABLE_EFFECT.Positive;
    //}
#endregion

#region Effects
    public void PreFeedSuccess(ActualGoapNode goapNode) {
        if (goapNode.poiTarget is Character targetCharacter) {
            targetCharacter.traitContainer.AddTrait(targetCharacter, "Eating");
        }
    }
    public void PerTickFeedSuccess(ActualGoapNode goapNode) {
        if (goapNode.poiTarget is Character targetCharacter) {
            Character actor = goapNode.actor;

            targetCharacter.needsComponent.AdjustFullness(20f);
        }
    }
    public void AfterFeedSuccess(ActualGoapNode goapNode) {
        if (goapNode.poiTarget is Character targetCharacter) {
            Character actor = goapNode.actor;

            targetCharacter.traitContainer.RemoveTrait(targetCharacter, "Eating");

            if (targetCharacter.traitContainer.HasTrait("Vampire")) {
                //If a vampre drinks the blood of another vampire and he is not a cannibal, add Poor Meal status
                if (!actor.race.IsSapient() || (actor.traitContainer.HasTrait("Vampire") && !targetCharacter.traitContainer.HasTrait("Cannibal"))) {
                    targetCharacter.traitContainer.AddTrait(targetCharacter, "Poor Meal", actor);
                }
                if (GameUtilities.RollChance(98)) {
                    actor.traitContainer.AddTrait(actor, "Lethargic", targetCharacter);
                    actor.traitContainer.GetTraitOrStatus<Trait>("Lethargic")?.SetGainedFromDoingAction(goapNode.action.goapType, goapNode.isStealth);
                } else {
                    if (actor.traitContainer.AddTrait(actor, "Vampire", targetCharacter)) {
                        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", goapName, "contracted", goapNode, LOG_TAG.Life_Changes);
                        log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                        log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        log.AddLogToDatabase();
                        PlayerManager.Instance.player.ShowNotificationFrom(actor, log, true);
                    }

                    if (actor.isNormalCharacter) {
                        Vampire vampireTrait = targetCharacter.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
                        if (vampireTrait != null) {
                            vampireTrait.AdjustNumOfConvertedVillagers(1);
                        }
                    }
                }
            }
        }
    }
#endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (poiTarget.gridTileLocation != null && actor != poiTarget) { //actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure
                return true;
            }
        }
        return false;
    }
#endregion
}