using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Drink : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.CONSUME; } }

    public Drink() : base(INTERACTION_TYPE.DRINK) {
        // validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.EARLY_NIGHT, TIME_IN_WORDS.LATE_NIGHT, TIME_IN_WORDS.AFTER_MIDNIGHT, };
        actionIconString = GoapActionStateDB.Drink_Icon;
        
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Drink Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}:";
        int cost = UtilityScripts.Utilities.Rng.Next(80, 121);
        costLog += $" +{cost}(Initial)";
        if (actor.traitContainer.HasTrait("Alcoholic")) {
            cost += -15;
            costLog += " -15(Alcoholic)";
        } else {
            int numOfTimesActionDone = actor.jobComponent.GetNumOfTimesActionDone(this);
            TIME_IN_WORDS timeOfDay = GameManager.GetCurrentTimeInWordsOfTick();
            if (timeOfDay == TIME_IN_WORDS.MORNING || timeOfDay == TIME_IN_WORDS.LUNCH_TIME ||  timeOfDay == TIME_IN_WORDS.AFTERNOON) {
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
    #endregion

    #region State Effects
    public void PreDrinkSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustDoNotGetBored(1);
        goapNode.actor.jobComponent.IncreaseNumOfTimesActionDone(this);
    }
    public void PerTickDrinkSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustHappiness(3.35f);
        goapNode.actor.needsComponent.AdjustComfort(2f);
        if (goapNode.poiTarget is Table) {
            Table table = goapNode.poiTarget as Table;
            table.AdjustResource(RESOURCE.FOOD, -1);
        }
    }
    public void AfterDrinkSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustDoNotGetBored(-1);
        goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Drunk");
        int chance = UnityEngine.Random.Range(0, 100);
        if ((goapNode.actor.moodComponent.moodState == MOOD_STATE.LOW && chance < 2) || goapNode.actor.moodComponent.moodState == MOOD_STATE.CRITICAL && chance < 4) {
            goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Drunkard");
        }
        //TODO: Remove all Withdrawal stacks
        
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
    //        log = new Log(GameManager.Instance.Today(), "GoapAction", "Drink", logKey, this);
    //        log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //    } else if (res == "Death") {
    //        log = new Log(GameManager.Instance.Today(), "GoapAction", "Drink", "drink poisoned_killed", this);
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
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
                return false;
            }
            return poiTarget.gridTileLocation != null && poiTarget.gridTileLocation.structure.structureType == STRUCTURE_TYPE.INN && poiTarget.IsAvailable() && !actor.traitContainer.HasTrait("Agoraphobic");
        }
        return false;
    }
    #endregion
}

public class DrinkData : GoapActionData {
    public DrinkData() : base(INTERACTION_TYPE.DRINK) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        return poiTarget.gridTileLocation != null &&  poiTarget.gridTileLocation.structure.structureType == STRUCTURE_TYPE.INN && poiTarget.IsAvailable() && !actor.traitContainer.HasTrait("Agoraphobic");
    }
}