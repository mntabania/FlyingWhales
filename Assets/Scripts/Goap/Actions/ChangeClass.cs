using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
public class ChangeClass : GoapAction {
    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public ChangeClass() : base(INTERACTION_TYPE.CHANGE_CLASS) {
        //actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.No_Icon;
        //actionIconString = GoapActionStateDB.Work_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        logTags = new[] {LOG_TAG.Life_Changes};
    }

    #region Overrides
    //protected override void ConstructBasePreconditionsAndEffects() {
    //    AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.CHANGE_CLASS, GOAP_EFFECT_TARGET.ACTOR));
    //}
    //protected override List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, OtherData[] otherData, out bool isOverridden) {
    //    if (otherData != null && otherData.Length > 0) {
    //        List<GoapEffect> ee = ObjectPoolManager.Instance.CreateNewExpectedEffectsList();
    //        List<GoapEffect> baseEE = base.GetExpectedEffects(actor, target, otherData, out isOverridden);
    //        if (baseEE != null && baseEE.Count > 0) {
    //            ee.AddRange(baseEE);
    //        }
    //        ee.Add(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.CHANGE_CLASS, conditionKey = (string) otherData[0].obj, target = GOAP_EFFECT_TARGET.ACTOR });
    //        isOverridden = true;
    //        return ee;
    //    }
    //    return base.GetExpectedEffects(actor, target, otherData, out isOverridden);
    //}
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Change Class Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        return 1;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        log.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters((string)node.otherData[0].obj), LOG_IDENTIFIER.STRING_1);
    }
    #endregion

    #region Effects
    public void AfterChangeClassSuccess(ActualGoapNode goapNode) {
        OtherData[] otherData = goapNode.otherData;
        string className = (string) otherData[0].obj;
        goapNode.actor.classComponent.AssignClass(className);
        //Reserve character after changing class
        //This is to prevent 1 character from changing class again immediately after changing class
        //Also to prevent 1 character from getting all change class jobs in settlement
        goapNode.actor.classComponent.SetShouldChangeClass(false);

        ManMadeStructure previousWorkStructure = goapNode.actor.structureComponent.workPlaceStructure;
        if (previousWorkStructure != null) {
            //if actor can no longer work at its current work structure, unassign them
            CharacterClassData classData = CharacterManager.Instance.GetOrCreateCharacterClassData(className);
            if (classData.workStructureType != previousWorkStructure.structureType) {
                previousWorkStructure.RemoveAssignedWorker(goapNode.actor);    
            }
        }
        
        if (otherData.Length > 1) {
            OtherData structureOtherData = otherData[1];
            if (structureOtherData != null) {
                ManMadeStructure workStructure = structureOtherData.obj as ManMadeStructure;
                if (workStructure != null && !workStructure.hasBeenDestroyed) {
                    //Upon changing class assign worker immediately to the attached structure
                    workStructure.AddAssignedWorker(goapNode.actor);
                    goapNode.actor.interruptComponent.TriggerInterrupt(INTERRUPT.Claim_Work_Structure, goapNode.actor);
                }
            }
        }
    }
    #endregion
}
