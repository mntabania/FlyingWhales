using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Logs;
using UnityEngine;

public class Visit : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.INDIRECT; } }

    public Visit() : base(INTERACTION_TYPE.VISIT) {
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION;
        actionIconString = GoapActionStateDB.Happy_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        // validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.MORNING, TIME_IN_WORDS.LUNCH_TIME, TIME_IN_WORDS.AFTERNOON, TIME_IN_WORDS.EARLY_NIGHT };
        doesNotStopTargetCharacter = true;
        logTags = new[] {LOG_TAG.Social};
    }

    #region Overrides
    public override LocationStructure GetTargetStructure(ActualGoapNode node) {
        OtherData[] otherData = node.otherData;
        if (otherData != null && otherData.Length >= 1) {
            //if (otherData[0] is Dwelling) {
            //    return otherData[0] as Dwelling;
            //} else 
            if (otherData[0].obj is LocationStructure) {
                return otherData[0].obj as LocationStructure;
            } 
        }
        return null;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        OtherData[] otherData = node.otherData;
        if (otherData != null && otherData.Length >= 1) {
            if (otherData[0].obj is LocationStructure) {
                LocationStructure structure = otherData[0].obj as LocationStructure; 
                log.AddToFillers(structure, structure.GetNameRelativeTo(node.actor), LOG_IDENTIFIER.LANDMARK_1);
            } 
        }
        
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Visit Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
#endregion

#region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (otherData.Length == 2) {
              //if provided other data is 2, assume that the second data is the target character, and check that the poi target is the same as that object
              IPointOfInterest targetObj = otherData[1].obj as IPointOfInterest;
              return poiTarget == targetObj;
            } else {
                return actor == poiTarget;    
            }
            
        }
        return false;
    }
#endregion

#region State Effects
    //public void PreVisitSuccess(ActualGoapNode goapNode) {
        //goapNode.descriptionLog.AddToFillers(null, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    public void AfterVisitSuccess(ActualGoapNode goapNode) {
        goapNode.actor.trapStructure.SetStructureAndDuration(goapNode.targetStructure, GameManager.Instance.GetTicksBasedOnHour(2) + GameManager.Instance.GetTicksBasedOnMinutes(30));
    }
#endregion
}
