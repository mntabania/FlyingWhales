using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;  
using Traits;
using Inner_Maps.Location_Structures;
using Inner_Maps;
using UtilityScripts;

public class BurnAtStake : GoapAction {

    public BurnAtStake() : base(INTERACTION_TYPE.BURN_AT_STAKE) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Burn_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        logTags = new[] {LOG_TAG.Work};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        SetPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), CanDoBurnAtStake);
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Burn Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
#endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (actor != poiTarget) {
                if(poiTarget is Character target) {
                    return !target.interruptComponent.isInterrupted && target.gridTileLocation != null && actor.homeSettlement != null;
                }
            }
            return false;
        }
        return false;
    }
#endregion

#region Preconditions
    private bool CanDoBurnAtStake(Character actor, IPointOfInterest target, object[] otherData, JOB_TYPE jobType) {
        return target is Character targetCharacter && target.traitContainer.HasTrait("Restrained") && targetCharacter.gridTileLocation.structure.structureType == STRUCTURE_TYPE.WILDERNESS;
    }
#endregion

#region State Effects
    public void AfterBurnSuccess(ActualGoapNode goapNode) {
        Character target = goapNode.target as Character;
        if (target.traitContainer.HasTrait("Criminal")) {
            Criminal criminalTrait = target.traitContainer.GetTraitOrStatus<Criminal>("Criminal");
            criminalTrait.SetIsImprisoned(false);
        }
        target.crimeComponent.SetDecisionAndJudgeToAllUnpunishedCrimesWantedBy(target.faction, CRIME_STATUS.Burned_At_Stake, goapNode.actor);
        target.crimeComponent.RemoveAllCrimesWantedBy(goapNode.actor.faction);
        target.traitContainer.RemoveRestrainAndImprison(target, goapNode.actor);

        Faction oldFaction = target.faction;
        oldFaction.KickOutCharacter(target);
        target.MigrateHomeStructureTo(null);
        target.ClearTerritory();

        target.interruptComponent.TriggerInterrupt(INTERRUPT.Burning_At_Stake, goapNode.actor);
    }
#endregion

}