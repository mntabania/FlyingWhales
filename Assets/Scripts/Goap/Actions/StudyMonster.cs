using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StudyMonster : GoapAction {

    public StudyMonster() : base(INTERACTION_TYPE.STUDY_MONSTER) {
        actionIconString = GoapActionStateDB.Inspect_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.DEMON };
        logTags = new[] {LOG_TAG.Work};
    }

    #region Override
    public override bool ShouldActionBeAnIntel(ActualGoapNode node) {
        return true;
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        SetPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Unconscious", target = GOAP_EFFECT_TARGET.TARGET }, HasUnconscious);
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Study Success", goapNode);
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

