using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using Inner_Maps;
using UtilityScripts;

public class Drink : GoapAction {

    public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.CONSUME;
    public Drink() : base(INTERACTION_TYPE.DRINK) {
        actionIconString = GoapActionStateDB.Drink_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        logTags = new[] {LOG_TAG.Needs};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Drink Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}:";
        if (actor.partyComponent.hasParty && actor.partyComponent.currentParty.isActive) {
            if (actor.partyComponent.isActiveMember) {
                if (target.gridTileLocation != null && target.gridTileLocation.collectionOwner.isPartOfParentRegionMap && actor.gridTileLocation != null
                && actor.gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
                    LocationGridTile centerGridTileOfTarget = target.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.GetCenterLocationGridTile();
                    LocationGridTile centerGridTileOfActor = actor.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.GetCenterLocationGridTile();
                    float distance = centerGridTileOfActor.GetDistanceTo(centerGridTileOfTarget);
                    int distanceToCheck = (InnerMapManager.BuildingSpotSize.x * 2) * 3;

                    if (distance > distanceToCheck) {
                        //target is at structure that character is avoiding
                        costLog += $" +2000(Active Party, Location of target too far from actor)";
                        actor.logComponent.AppendCostLog(costLog);
                        return 2000;
                    }
                }
            }
        }
        int cost = UtilityScripts.Utilities.Rng.Next(80, 121);
        costLog += $" +{cost}(Initial)";
        if (actor.traitContainer.HasTrait("Alcoholic")) {
            cost -= 35;
            costLog += " -35(Alcoholic)";
        } else {
            int numOfTimesActionDone = actor.jobComponent.GetNumOfTimesActionDone(this);
            TIME_IN_WORDS timeOfDay = GameManager.GetCurrentTimeInWordsOfTick();
            if (timeOfDay == TIME_IN_WORDS.MORNING ||  timeOfDay == TIME_IN_WORDS.AFTERNOON) {
                cost += 2000;
                costLog += " +2000(not Alcoholic, Morning/Lunch/Afternoon)";
            }
            if (numOfTimesActionDone > 5) {
                cost += 2000;
                costLog += " +2000(Times Drank > 5)";
            } else {
                int timesCost = 10 * numOfTimesActionDone;
                cost += timesCost;
                costLog += $" +{timesCost}(10 x Times Drank)";
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
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        if (witness.traitContainer.HasTrait("Alcoholic")) {
            return REACTABLE_EFFECT.Positive;
        }
        return REACTABLE_EFFECT.Neutral;
    }
    #endregion

    #region State Effects
    public void PreDrinkSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustDoNotGetBored(1);
        goapNode.actor.jobComponent.IncreaseNumOfTimesActionDone(this);
    }
    public void PerTickDrinkSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustHappiness(goapNode.actor.traitContainer.HasTrait("Alcoholic") ? 0.5f : 1f);
        // goapNode.actor.needsComponent.AdjustStamina(2f);
        // if (goapNode.poiTarget is Table) {
        //     Table table = goapNode.poiTarget as Table;
        //     table.AdjustResource(RESOURCE.FOOD, -1);
        // }
    }
    public void AfterDrinkSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustDoNotGetBored(-1);
        goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Drunk");
        if ((goapNode.actor.moodComponent.moodState == MOOD_STATE.Bad && GameUtilities.RollChance(2)) || goapNode.actor.moodComponent.moodState == MOOD_STATE.Critical && GameUtilities.RollChance(4)) {
            goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Alcoholic");
        }
        goapNode.actor.traitContainer.RemoveStatusAndStacks(goapNode.actor, "Withdrawal");
        
    }
    //public void PreDrinkPoisoned() {
    //    actor.AdjustDoNotGetBored(1);
    //    RemoveTraitFrom(poiTarget, "Poisoned");
    //    Log log = null;
    //    WeightedDictionary<string> result = poisonedTrait.GetResultWeights();
    //    string res = result.PickRandomElementGivenWeights();
    //    if (res == "Sick") {
    //        string logKey = "drink poisoned_sick";
    //        poisonedResult = "Sick";
    //        if (actor.traitContainer.GetNormalTrait<Trait>("Robust") != null) {
    //            poisonedResult = "Robust";
    //            logKey = "drink poisoned_robust";
    //        }
    //        log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", "Drink", logKey, this);
    //        log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //    } else if (res == "Death") {
    //        log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", "Drink", "drink poisoned_killed", this);
    //        log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //        poisonedResult = "Death";
    //    }
    //    currentState.OverrideDescriptionLog(log);
    //}
    //public void PerTickDrinkPoisoned() {
    //    actor.AdjustHappiness(200);
    //}
    //public void AfterDrinkPoisoned() {
    //    actor.AdjustDoNotGetBored(-1);
    //    if (poisonedResult == "Sick") {
    //        for (int i = 0; i < poisonedTrait.responsibleCharacters.Count; i++) {
    //            AddTraitTo(actor, poisonedResult, poisonedTrait.responsibleCharacters[i]);
    //        }
    //    } else if (poisonedResult == "Death") {
    //        if (parentPlan.job != null) {
    //            parentPlan.job.SetCannotCancelJob(true);
    //        }
    //        SetCannotCancelAction(true);
    //        actor.Death("poisoned", deathFromAction: this);
    //    }
    //}
    //public void PreTargetMissing() {
    //    actor.RemoveAwareness(poiTarget);
    //}
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapStructureIsNot(poiTarget.gridTileLocation.structure)) {
                return false;
            }
            if (poiTarget.gridTileLocation != null && poiTarget.gridTileLocation.collectionOwner.isPartOfParentRegionMap && actor.trapStructure.IsTrappedAndTrapHexIsNot(poiTarget.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner)) {
                return false;
            }
            return poiTarget.gridTileLocation != null && poiTarget.gridTileLocation.structure.structureType == STRUCTURE_TYPE.TAVERN && poiTarget.IsAvailable() && !actor.traitContainer.HasTrait("Agoraphobic");
        }
        return false;
    }
    #endregion
}