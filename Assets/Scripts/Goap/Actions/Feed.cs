using System;
using System.Collections;
using System.Collections.Generic;
using Goap.Unique_Action_Data;
using UnityEngine;  
using Traits;

public class Feed : GoapAction {

    public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;
    public override Type uniqueActionDataType => typeof(FeedUAD);
    public Feed() : base(INTERACTION_TYPE.FEED) {
        actionIconString = GoapActionStateDB.FirstAid_Icon;
        doesNotStopTargetCharacter = true;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Needs};
    }

    #region Overrides
    public override bool ShouldActionBeAnIntel(ActualGoapNode node) {
        return true;
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        SetPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.FEED, "Food Pile", false, GOAP_EFFECT_TARGET.ACTOR), ActorHasFood);
        //SetPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TAKE_POI, conditionKey = "Food Pile", isKeyANumber = false, target = GOAP_EFFECT_TARGET.ACTOR }, ActorHasFood);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.TARGET });
    }
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
    public override void OnStopWhileStarted(ActualGoapNode node) {
        base.OnStopWhileStarted(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        actor.UncarryPOI();
        poiTarget.traitContainer.RemoveTrait(poiTarget, "Eating");
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        actor.UncarryPOI();
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
    public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
        if (target is Character targetCharacter) {
            FeedUAD uniqueData = node.GetConvertedUniqueActionData<FeedUAD>();
            string opinionOfTarget = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
            if (uniqueData.usedPoisonedFood) {
                reactions.Add(EMOTION.Shock);
                if (opinionOfTarget == RelationshipManager.Friend || opinionOfTarget == RelationshipManager.Close_Friend) {
                    reactions.Add(EMOTION.Anger);
                }
            } else {
                if (opinionOfTarget == RelationshipManager.Friend || opinionOfTarget == RelationshipManager.Close_Friend) {
                    if (!witness.traitContainer.HasTrait("Psychopath")) {
                        reactions.Add(EMOTION.Gratefulness);
                    }
                } else if (opinionOfTarget == RelationshipManager.Rival) {
                    reactions.Add(EMOTION.Disapproval);
                }
            }
        }
    }
    public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
        if (target is Character targetCharacter) {
            FeedUAD uniqueData = node.GetConvertedUniqueActionData<FeedUAD>();
            if (uniqueData.usedPoisonedFood) {
                reactions.Add(EMOTION.Shock);
                reactions.Add(EMOTION.Betrayal);
            } else {
                if (!targetCharacter.traitContainer.HasTrait("Psychopath")) {
                    if (targetCharacter.relationshipContainer.IsEnemiesWith(actor)) {
                        if (UnityEngine.Random.Range(0, 100) < 30) {
                            reactions.Add(EMOTION.Gratefulness);
                        }
                        if (UnityEngine.Random.Range(0, 100) < 20) {
                            reactions.Add(EMOTION.Embarassment);
                        }
                    } else {
                        reactions.Add(EMOTION.Gratefulness);
                    }
                }
            }
        }
    }
    public override void OnActionStarted(ActualGoapNode node) {
        base.OnActionStarted(node);
        for (int i = 0; i < node.actor.items.Count; i++) {
            TileObject tileObject = node.actor.items[i];
            if(tileObject.resourceStorageComponent.HasResourceAmount(RESOURCE.FOOD, 10)) {
                node.actor.ShowItemVisualCarryingPOI(tileObject);
                break;
            }
        }
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        FeedUAD uniqueData = node.GetConvertedUniqueActionData<FeedUAD>();
        if (uniqueData.usedPoisonedFood) {
            return REACTABLE_EFFECT.Negative;
        }
        return REACTABLE_EFFECT.Positive;
    }
#endregion

#region Effects
    public void PreFeedSuccess(ActualGoapNode goapNode) {
        if (goapNode.poiTarget is Character targetCharacter) {
            targetCharacter.traitContainer.AddTrait(targetCharacter, "Eating");
            if(goapNode.actor.carryComponent.carriedPOI is ResourcePile carriedPile) {
                FeedUAD uniqueData = goapNode.GetConvertedUniqueActionData<FeedUAD>();
                if (carriedPile.traitContainer.HasTrait("Poisoned")) {
                    uniqueData.SetUsedPoisonedFood(true);
                    Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", "Feed", "used_poison", goapNode, logTags);
                    log.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    goapNode.OverrideDescriptionLog(log);
                }
                carriedPile.AdjustResourceInPile(-20);
                // targetCharacter.resourceStorageComponent.AdjustResource(RESOURCE.FOOD, 12);
            }    
        }
    }
    public void PerTickFeedSuccess(ActualGoapNode goapNode) {
        if (goapNode.poiTarget is Character targetCharacter) {
            FeedUAD uniqueData = goapNode.GetConvertedUniqueActionData<FeedUAD>();
            if (uniqueData.usedPoisonedFood) {
                targetCharacter.AdjustHP(-100, ELEMENTAL_TYPE.Normal, triggerDeath: true);  
            }
            targetCharacter.needsComponent.AdjustFullness(5f);
            // targetCharacter.resourceStorageComponent.AdjustResource(RESOURCE.FOOD, -1);
        }
        
        
    }
    public void AfterFeedSuccess(ActualGoapNode goapNode) {
        if (goapNode.poiTarget is Character targetCharacter) {
            FeedUAD uniqueData = goapNode.GetConvertedUniqueActionData<FeedUAD>();
            targetCharacter.traitContainer.RemoveTrait(targetCharacter, "Eating");
            if (goapNode.actor != targetCharacter) {
                if (uniqueData.usedPoisonedFood) {
                    targetCharacter.relationshipContainer.AdjustOpinion(targetCharacter, goapNode.actor, "Poisoned me.", -10);
                } else {
                    targetCharacter.relationshipContainer.AdjustOpinion(targetCharacter, goapNode.actor, "Helped me.", 5);    
                }
            }
            if (uniqueData.usedPoisonedFood) {
                targetCharacter.traitContainer.AddTrait(targetCharacter, "Poisoned", goapNode.actor, bypassElementalChance: true);
            }
        }
        goapNode.actor.UncarryPOI();
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

#region Preconditions
    private bool ActorHasFood(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        if (poiTarget.resourceStorageComponent.HasResourceAmount(RESOURCE.FOOD, 10)) {
            return true;
        }
        if(actor.items.Count > 0) {
            for (int i = 0; i < actor.items.Count; i++) {
                if(actor.items[i].resourceStorageComponent.HasResourceAmount(RESOURCE.FOOD, 10)) {
                    return true;
                }
            }
        }
        if (actor.carryComponent.isCarryingAnyPOI && actor.carryComponent.carriedPOI is FoodPile) {
            //ResourcePile carriedPile = actor.ownParty.carriedPOI as ResourcePile;
            //return carriedPile.resourceInPile >= 12;
            return true;
        }
        return false;
        //return actor.supply >= 20;
    }
#endregion
}