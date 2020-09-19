using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeClass : GoapAction {
    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public ChangeClass() : base(INTERACTION_TYPE.CHANGE_CLASS) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.No_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        logTags = new[] {LOG_TAG.Life_Changes};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.CHANGE_CLASS, GOAP_EFFECT_TARGET.ACTOR));
    }
    protected override List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, OtherData[] otherData) {
        List<GoapEffect> ee = base.GetExpectedEffects(actor, target, otherData);
        if(otherData != null && otherData.Length > 0) {
            ee.Add(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.CHANGE_CLASS, conditionKey = (string) otherData[0].obj, target = GOAP_EFFECT_TARGET.ACTOR });
        }
        return ee;
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Change Class Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        return 1;
    }
    #endregion

    #region Effects
    public void PreChangeClassSuccess(ActualGoapNode goapNode) {
        goapNode.descriptionLog.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters((string)goapNode.otherData[0].obj), LOG_IDENTIFIER.STRING_1);
    }
    public void AfterChangeClassSuccess(ActualGoapNode goapNode) {
        string className = (string) goapNode.otherData[0].obj;
        goapNode.actor.AssignClass(className);
    }
    #endregion
}
