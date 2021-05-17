using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Locations.Area_Features;
using Logs;
using UnityEngine;
using Traits;

public class TakeShelter : GoapAction {

    public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.INDIRECT;
    public TakeShelter() : base(INTERACTION_TYPE.TAKE_SHELTER) {
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION;
        actionIconString = GoapActionStateDB.Cowering_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        doesNotStopTargetCharacter = true;
        logTags = new[] {LOG_TAG.Needs};
    }

    #region Overrides
    public override LocationStructure GetTargetStructure(ActualGoapNode node) {
        OtherData[] otherData = node.otherData;
        if (otherData != null && otherData.Length == 2) {
            if (otherData[0].obj is LocationStructure) {
                return otherData[0].obj as LocationStructure;
            } 
        }
        return null;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        OtherData[] otherData = node.otherData;
        if (otherData != null && otherData.Length == 2) {
            if (otherData[0].obj is LocationStructure structure) {
                log.AddToFillers(structure, structure.GetNameRelativeTo(node.actor), LOG_IDENTIFIER.LANDMARK_1);
            }
            if (otherData[1] is StringOtherData stringOtherData) {
                log.AddToFillers(null, stringOtherData.str, LOG_IDENTIFIER.STRING_1);
            } 
        }
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Take Shelter Success", goapNode);
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
            return actor == poiTarget;
        }
        return false;
    }
#endregion

#region State Effects
    public void AfterTakeShelterSuccess(ActualGoapNode goapNode) {
        bool shouldSetShelter = goapNode.actor.gridTileLocation != null && 
            (goapNode.actor.areaLocation.featureComponent.HasFeature(AreaFeatureDB.Blizzard_Feature) 
            || goapNode.actor.areaLocation.featureComponent.HasFeature(AreaFeatureDB.Heat_Wave_Feature));
        if (shouldSetShelter) {
            if (goapNode.actor.traitContainer.HasTrait("Freezing")) {
                Freezing freezing = goapNode.actor.traitContainer.GetTraitOrStatus<Freezing>("Freezing");
                freezing.SetCurrentShelterStructure(goapNode.targetStructure);
            }
            if (goapNode.actor.traitContainer.HasTrait("Overheating")) {
                Overheating overheating = goapNode.actor.traitContainer.GetTraitOrStatus<Overheating>("Overheating");
                overheating.SetCurrentShelterStructure(goapNode.targetStructure);
            }
            goapNode.actor.trapStructure.SetForcedStructure(goapNode.targetStructure);
        }
    }
#endregion
}
