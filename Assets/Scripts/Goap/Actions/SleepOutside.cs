using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class SleepOutside : GoapAction {

    public SleepOutside() : base(INTERACTION_TYPE.SLEEP_OUTSIDE) {
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION_B;
        actionIconString = GoapActionStateDB.Sleep_Icon;
        //animationName = "Sleep Ground";
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] {
            RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON,
            RACE.GOLEM, RACE.KOBOLD, RACE.LESSER_DEMON, RACE.MIMIC, RACE.PIG, RACE.SHEEP, RACE.ENT, RACE.WISP,
            RACE.GHOST, RACE.NYMPH, RACE.SLIME, RACE.SLUDGE, RACE.CHICKEN, RACE.ELEMENTAL, RACE.ABOMINATION, RACE.ANGEL, RACE.DEMON, RACE.RATMAN
        };
        logTags = new[] {LOG_TAG.Needs};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = string.Empty, target = GOAP_EFFECT_TARGET.ACTOR });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.STAMINA_RECOVERY, conditionKey = string.Empty, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Rest Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +160(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 160;
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        actor.traitContainer.RemoveTrait(actor, "Resting");
    }
    public override LocationStructure GetTargetStructure(ActualGoapNode node) {
        if (node.actor.homeStructure != null && node.actor.homeStructure.structureType == STRUCTURE_TYPE.DWELLING) {
            //Reference: https://trello.com/c/SQeH6f8c/4972-villagers-should-sleep-in-house
            return node.actor.homeStructure;
        }
        return base.GetTargetStructure(node);
    }
    public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode) {
        if (goapNode.actor.homeStructure != null && goapNode.actor.homeStructure.structureType == STRUCTURE_TYPE.DWELLING) {
            //Reference: https://trello.com/c/SQeH6f8c/4972-villagers-should-sleep-in-house
            if (goapNode.actor.isAtHomeStructure) {
                return goapNode.actor.gridTileLocation; //sleep in place        
            }
            return null; //Returned null so that Random Location B Logic will be applied to inside the actors house
        }
        return goapNode.actor.gridTileLocation; //sleep in place
    }
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
            return actor == poiTarget;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreRestSuccess(ActualGoapNode goapNode) {
        goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Resting");
        //GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
        //goapNode.OverrideCurrentStateDuration(currentState.duration - goapNode.actor.currentSleepTicks); //this can make the current duration negative
    }
    public void PerTickRestSuccess(ActualGoapNode goapNode) {
        Character actor = goapNode.actor;
        CharacterNeedsComponent needsComponent = actor.needsComponent;
        // if (needsComponent.currentSleepTicks == 1) { //If sleep ticks is down to 1 tick left, set current duration to end duration so that the action will end now, we need this because the character must only sleep the remaining hours of his sleep if ever that character is interrupted while sleeping
        //     goapNode.OverrideCurrentStateDuration(goapNode.currentState.duration);
        // }
        needsComponent.AdjustTiredness(0.25f);
        if (actor.race != RACE.RATMAN && !actor.partyComponent.isActiveMember) {
            needsComponent.AdjustHappiness(-0.2f);    
        }
        // needsComponent.AdjustSleepTicks(-1);
        //needsComponent.AdjustStamina(0.2f);
    }
    public void AfterRestSuccess(ActualGoapNode goapNode) {
        goapNode.actor.traitContainer.RemoveTrait(goapNode.actor, "Resting");
    }
    #endregion
}