using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Accident : GoapAction {
    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public Accident() : base(INTERACTION_TYPE.ACCIDENT) {
        actionIconString = GoapActionStateDB.No_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REDUCE_HP, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode actionNode) {
        base.Perform(actionNode);
        SetState("Accident Success", actionNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +5(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 5;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return actor == poiTarget;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreAccidentSuccess(ActualGoapNode goapNode) {
        GoapAction actionToDo = goapNode.otherData[0] as GoapAction;
        goapNode.descriptionLog.AddToFillers(actionToDo, actionToDo.goapName, LOG_IDENTIFIER.STRING_1);
    }
    public void AfterAccidentSuccess(ActualGoapNode goapNode) {
        goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Injured", gainedFromDoing: goapNode);

        int randomHpToLose = UnityEngine.Random.Range(5, 26);
        float percentMaxHPToLose = randomHpToLose / 100f;
        int actualHPToLose = Mathf.CeilToInt(goapNode.actor.maxHP * percentMaxHPToLose);
        Debug.Log(
            $"Accident of {goapNode.actor.name} percent: {percentMaxHPToLose}, max hp: {goapNode.actor.maxHP}, lost hp: {actualHPToLose}");
        goapNode.actor.AdjustHP(-actualHPToLose, ELEMENTAL_TYPE.Normal);
        if (goapNode.actor.currentHP <= 0) {
            goapNode.actor.Death(deathFromAction: goapNode);
        }
    }
    #endregion
}

public class AccidentData : GoapActionData {
    public AccidentData() : base(INTERACTION_TYPE.ACCIDENT) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor == poiTarget;
    }
}