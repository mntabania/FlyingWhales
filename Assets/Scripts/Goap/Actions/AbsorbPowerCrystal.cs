
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

public class AbsorbPowerCrystal : GoapAction {

    public AbsorbPowerCrystal() : base(INTERACTION_TYPE.ABSORB_POWER_CRYSTAL) {
        actionIconString = GoapActionStateDB.Magic_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.ELVES, };
        logTags = new[] { LOG_TAG.Work };
        //Do not add log because absorb power crystal log is done in AbsorbCrystal in Character script
        //If we add log here, it creates an absorb power crystal duplicate log
        shouldAddLogs = false;
    }

    #region Overrides
    //protected override void ConstructBasePreconditionsAndEffects() {
    //    AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.TARGET }, IsTargetDead);
    //    AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.ABSORB_LIFE, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
    //}
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Absorb Crystal Success", goapNode);
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
        
        return satisfied;
    }
    #endregion

    //#region Preconditions
    //private bool IsTargetDead(Character actor, IPointOfInterest poiTarget, object[] otherData) {
    //    if (poiTarget is Character character) {
    //        return character.isDead;
    //    }
    //    return true;
    //}
    //#endregion

    #region State Effects
    //public void PreAbsorbCrystalSuccess(ActualGoapNode goapNode) {
    //    //Character targetCharacter = goapNode.poiTarget as Character;
    //    //goapNode.descriptionLog.AddToFillers(null, targetCharacter.name + "POWER CRYSTAL ABSORB", LOG_IDENTIFIER.STRING_1);
    //}
    public void AfterAbsorbCrystalSuccess(ActualGoapNode goapNode) {
        //goapNode.actor.necromancerTrait.AdjustLifeAbsorbed(2);
        //(goapNode.poiTarget as Character).Death(deathFromAction: goapNode);

        Character targetCharacter = goapNode.actor as Character;
        TileObject p_crystal = goapNode.target as TileObject;
        targetCharacter.AbsorbCrystal(p_crystal as PowerCrystal);
        p_crystal.currentStructure?.RemovePOI(p_crystal);
    }
    #endregion
}