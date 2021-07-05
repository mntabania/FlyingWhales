using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using Inner_Maps;
using UtilityScripts;
using Locations.Settlements;
using Inner_Maps.Location_Structures;
public class Drink : GoapAction {

    public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.CONSUME;
    public Drink() : base(INTERACTION_TYPE.DRINK) {
        actionIconString = GoapActionStateDB.Drink_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
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
        BaseSettlement settlement = null;
        if(target.gridTileLocation != null && target.gridTileLocation.IsPartOfSettlement(out settlement)) {
            Faction targetFaction = settlement.owner;
            if(actor.faction != null && targetFaction != null && actor.faction.IsHostileWith(targetFaction)) {
                //Do not drink on hostile faction's taverns
#if DEBUG_LOG
                costLog += $" +2000(Location of target is in hostile faction of actor)";
                actor.logComponent.AppendCostLog(costLog);
#endif
                return 2000;
            }
        }
        if (actor.partyComponent.hasParty && actor.partyComponent.currentParty.isActive) {
            if (actor.partyComponent.isActiveMember) {
                if (target.gridTileLocation != null && actor.gridTileLocation != null) {
                    LocationGridTile centerGridTileOfTarget = target.gridTileLocation.area.gridTileComponent.centerGridTile;
                    LocationGridTile centerGridTileOfActor = actor.areaLocation.gridTileComponent.centerGridTile;
                    float distance = centerGridTileOfActor.GetDistanceTo(centerGridTileOfTarget);
                    int distanceToCheck = InnerMapManager.AreaLocationGridTileSize.x * 3;

                    if (distance > distanceToCheck) {
                        //target is at structure that character is avoiding
#if DEBUG_LOG
                        costLog += $" +2000(Active Party, Location of target too far from actor)";
                        actor.logComponent.AppendCostLog(costLog);
#endif
                        return 2000;
                    }
                }
            }
        }
        int cost = UtilityScripts.Utilities.Rng.Next(80, 121);
#if DEBUG_LOG
        costLog += $" +{cost}(Initial)";
#endif
        if (actor.traitContainer.HasTrait("Alcoholic")) {
            cost -= 35;
#if DEBUG_LOG
            costLog += " -35(Alcoholic)";
#endif
        } else {
            int numOfTimesActionDone = actor.jobComponent.GetNumOfTimesActionDone(this);
            TIME_IN_WORDS timeOfDay = GameManager.Instance.GetCurrentTimeInWordsOfTick();
            if (timeOfDay == TIME_IN_WORDS.MORNING ||  timeOfDay == TIME_IN_WORDS.AFTERNOON) {
                cost += 2000;
#if DEBUG_LOG
                costLog += " +2000(not Alcoholic, Morning/Lunch/Afternoon)";
#endif
            }
            if (numOfTimesActionDone > 5) {
                cost += 2000;
#if DEBUG_LOG
                costLog += " +2000(Times Drank > 5)";
#endif
            } else {
                int timesCost = 10 * numOfTimesActionDone;
                cost += timesCost;
#if DEBUG_LOG
                costLog += $" +{timesCost}(10 x Times Drank)";
#endif
            }
        }
#if DEBUG_LOG
        actor.logComponent.AppendCostLog(costLog);
#endif
        return cost;
    }
    //public override void OnStopWhilePerforming(ActualGoapNode node) {
    //    base.OnStopWhilePerforming(node);
    //    Character actor = node.actor;
    //    actor.needsComponent.AdjustDoNotGetBored(-1);
    //}
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        if (witness.traitContainer.HasTrait("Alcoholic")) {
            return REACTABLE_EFFECT.Positive;
        }
        return REACTABLE_EFFECT.Neutral;
    }
    public override bool IsHappinessRecoveryAction() {
        return true;
    }
#endregion

    #region State Effects
    public void PreDrinkSuccess(ActualGoapNode goapNode) {
        //goapNode.actor.needsComponent.AdjustDoNotGetBored(1);
        goapNode.actor.jobComponent.IncreaseNumOfTimesActionDone(this);
        LocationStructure targetStructure = goapNode.poiTarget.gridTileLocation?.structure;
        if (targetStructure != null && targetStructure.structureType == STRUCTURE_TYPE.TAVERN) {
            if (targetStructure is ManMadeStructure mmStructure) {
                if (mmStructure.HasAssignedWorker()) {
                    //only added coins to first worker since we expect that the tavern only has 1 worker.
                    //if that changes, this needs to be changed as well.
                    string assignedWorkerID = mmStructure.assignedWorkerIDs[0];
                    Character assignedWorker = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(assignedWorkerID);
                    assignedWorker.moneyComponent.AdjustCoins(33);
                }
                // Character assignedWorker = mmStructure.assignedWorker;
                // if (assignedWorker != null) {
                //     assignedWorker.moneyComponent.AdjustCoins(10);
                // }
            }
        }
    }
    public void PerTickDrinkSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustHappiness(goapNode.actor.traitContainer.HasTrait("Alcoholic") ? 1.35f : 2f);
        // goapNode.actor.needsComponent.AdjustStamina(2f);
        // if (goapNode.poiTarget is Table) {
        //     Table table = goapNode.poiTarget as Table;
        //     table.AdjustResource(RESOURCE.FOOD, -1);
        // }
    }
    public void AfterDrinkSuccess(ActualGoapNode goapNode) {
        //goapNode.actor.needsComponent.AdjustDoNotGetBored(-1);
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
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapStructureIsNot(poiTarget.gridTileLocation.structure)) {
                return false;
            }
            if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapAreaIsNot(poiTarget.gridTileLocation.area)) {
                return false;
            }
            return poiTarget.gridTileLocation != null && poiTarget.gridTileLocation.structure.structureType == STRUCTURE_TYPE.TAVERN && poiTarget.IsAvailable() && !actor.traitContainer.HasTrait("Agoraphobic");
        }
        return false;
    }
    #endregion
}