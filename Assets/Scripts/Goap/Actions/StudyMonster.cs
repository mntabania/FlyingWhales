using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StudyMonster : GoapAction {

    public StudyMonster() : base(INTERACTION_TYPE.STUDY_MONSTER) {
        actionIconString = GoapActionStateDB.Work_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.DEMON };
        isNotificationAnIntel = true;
    }

    #region Override
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Unconscious", target = GOAP_EFFECT_TARGET.TARGET }, HasUnconscious);
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Study Success", goapNode);
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
            return poiTarget != actor && poiTarget.gridTileLocation != null;
        }
        return false;
    }
    #endregion

    #region Preconditions
    private bool HasUnconscious(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        Character target = poiTarget as Character;
        return target.traitContainer.HasTrait("Unconscious");
    }
    #endregion

    #region State Effects
    public void AfterStudySuccess(ActualGoapNode goapNode) {
        //IPointOfInterest target = goapNode.poiTarget;
        //if(target is Character) {
        //    Character targetCharacter = target as Character;
        //    PlayerManager.Instance.player.archetype.AddMonster(new RaceClass(targetCharacter.race, targetCharacter.characterClass.className));
        //}
    }
    #endregion
}

