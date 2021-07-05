using System.Diagnostics;

public class CarryPatient : GoapAction {
    
    public CarryPatient() : base(INTERACTION_TYPE.CARRY_PATIENT) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Haul_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.LESSER_DEMON };
        logTags = new[] {LOG_TAG.Life_Changes, LOG_TAG.Social};
        canBeAdvertisedEvenIfTargetIsUnavailable = true;
    }
    
    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.CARRIED_PATIENT, string.Empty, false, GOAP_EFFECT_TARGET.TARGET));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Carry Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest poiTarget, JobQueueItem job, OtherData[] otherData) {
        return 1;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            Character targetCharacter = poiTarget as Character;
            Debug.Assert(targetCharacter != null, nameof(targetCharacter) + " != null");
            if (targetCharacter.combatComponent.isInCombat
                || (targetCharacter.stateComponent.currentState != null && targetCharacter.stateComponent.currentState.characterState == CHARACTER_STATE.DOUSE_FIRE)
                || (targetCharacter.interruptComponent.isInterrupted && targetCharacter.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Cowering)) {
#if DEBUG_LOG
                string debugLog = $"{targetCharacter.name}in combat/in douse fire state/cowering. Carry fail.";
                actor.logComponent.PrintLogIfActive(debugLog);
#endif
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.reason = "target_unavailable";
            }
        }
        return goapActionInvalidity;
    }
    #endregion

    #region State Effects
    public void PreCarrySuccess(ActualGoapNode goapNode) {
        goapNode.actor.CarryPOI(goapNode.poiTarget);
    }
    #endregion
    
    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            Character target = poiTarget as Character;
            if (target == actor) {
                return false;
            }
            if (target.stateComponent.currentState is CombatState) { //do not carry characters that are currently in combat
                return false;
            }
            if (target.carryComponent.masterCharacter.movementComponent.isTravellingInWorld || target.currentRegion != actor.currentRegion) {
                return false; //target is outside the map
            }
            if (actor.homeSettlement == null) {
                return false;
            }
            if (!actor.homeSettlement.HasStructure(STRUCTURE_TYPE.HOSPICE)) {
                return false;
            }
            return target.carryComponent.IsNotBeingCarried();
        }
        return false;
    }
    #endregion
}
