
using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class AbsorbPower : GoapAction {

    public AbsorbPower() : base(INTERACTION_TYPE.ABSORB_POWER) {
        actionIconString = GoapActionStateDB.Magic_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON, RACE.DEMON };
    }

    #region Overrides
    //protected override void ConstructBasePreconditionsAndEffects() {
    //    AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.TARGET }, IsTargetDead);
    //    AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.ABSORB_LIFE, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
    //}
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Absorb Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if(poiTarget is Character targetcharacter) {
                if(targetcharacter.characterClass.elementalType != ELEMENTAL_TYPE.Normal) {
                    return actor != poiTarget && poiTarget.mapObjectVisual;
                }
            }
        }
        return false;
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
    public void PreAbsorbSuccess(ActualGoapNode goapNode) {
        Character targetCharacter = goapNode.poiTarget as Character;
        goapNode.descriptionLog.AddToFillers(null, targetCharacter.characterClass.elementalType.ToString(), LOG_IDENTIFIER.STRING_1);
    }
    public void AfterAbsorbSuccess(ActualGoapNode goapNode) {
        //goapNode.actor.necromancerTrait.AdjustLifeAbsorbed(2);
        //(goapNode.poiTarget as Character).Death(deathFromAction: goapNode);

        Character targetCharacter = goapNode.poiTarget as Character;
        goapNode.actor.traitContainer.AddTrait(goapNode.actor, targetCharacter.characterClass.elementalType.ToString() + " Attacker");

        if (targetCharacter.marker) {
            targetCharacter.DestroyMarker();
        }
    }
    #endregion

}