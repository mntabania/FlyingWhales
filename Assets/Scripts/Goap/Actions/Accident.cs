using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

[System.Obsolete("This is no longer advertised by anything.")]
public class Accident : GoapAction {
    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public Accident() : base(INTERACTION_TYPE.ACCIDENT) {
        actionIconString = GoapActionStateDB.No_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Life_Changes};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.REDUCE_HP, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
    }
    public override void Perform(ActualGoapNode actionNode) {
        base.Perform(actionNode);
        SetState("Accident Success", actionNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +5(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 5;
    }
#endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            return actor == poiTarget;
        }
        return false;
    }
#endregion

#region State Effects
    public void PreAccidentSuccess(ActualGoapNode goapNode) {
        GoapAction actionToDo = goapNode.otherData[0].obj as GoapAction;
        // goapNode.descriptionLog.AddToFillers(actionToDo, actionToDo.goapName, LOG_IDENTIFIER.STRING_1);
    }
    public void AfterAccidentSuccess(ActualGoapNode goapNode) {
        goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Injured");
        Trait trait = goapNode.actor.traitContainer.GetTraitOrStatus<Trait>("Injured");
        if (trait != null) {
            trait.SetGainedFromDoingAction(goapNode.action.goapType, goapNode.isStealth);
        }

        int randomHpToLose = UnityEngine.Random.Range(5, 26);
        float percentMaxHPToLose = randomHpToLose / 100f;
        int actualHPToLose = Mathf.CeilToInt(goapNode.actor.maxHP * percentMaxHPToLose);
#if DEBUG_LOG
        Debug.Log(
            $"Accident of {goapNode.actor.name} percent: {percentMaxHPToLose}, max hp: {goapNode.actor.maxHP}, lost hp: {actualHPToLose}");
#endif
        goapNode.actor.AdjustHP(-actualHPToLose, ELEMENTAL_TYPE.Normal, showHPBar: true);
        if (!goapNode.actor.HasHealth()) {
            goapNode.actor.Death(deathFromAction: goapNode);
        }
    }
#endregion
}