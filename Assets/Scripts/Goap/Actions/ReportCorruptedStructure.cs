using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;
using Traits;
using Inner_Maps;

public class ReportCorruptedStructure : GoapAction {

    public ReportCorruptedStructure() : base(INTERACTION_TYPE.REPORT_CORRUPTED_STRUCTURE) {
        actionIconString = GoapActionStateDB.Work_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES };
    }

    #region Override
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Report Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override LocationStructure GetTargetStructure(ActualGoapNode node) {
        Character actor = node.actor;
        return actor.homeSettlement.cityCenter;
    }
    public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode) {
        Character actor = goapNode.actor;
        return actor.homeSettlement.cityCenter.GetRandomTile();
    }
    public override IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode) {
        return null;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        LocationStructure structureToReport = node.otherData[0] as LocationStructure;
        log.AddToFillers(structureToReport, structureToReport.GetNameRelativeTo(node.actor), LOG_IDENTIFIER.LANDMARK_2);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return poiTarget == actor && poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
        }
        return false;
    }
    #endregion

    #region State Effects
    //public void PreReportSuccess(ActualGoapNode goapNode) {
    //    object[] otherData = goapNode.otherData;
    //    LocationStructure structureToReport = otherData[0] as LocationStructure;
    //    goapNode.descriptionLog.AddToFillers(structureToReport, structureToReport.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_2);
    //}
    public void AfterReportSuccess(ActualGoapNode goapNode) {
        object[] otherData = goapNode.otherData;
        LocationStructure structureToReport = otherData[0] as LocationStructure;
        if (!InnerMapManager.Instance.HasWorldKnownDemonicStructure(structureToReport)) {
            InnerMapManager.Instance.AddWorldKnownDemonicStructure(structureToReport);
            PlayerManager.Instance.player.threatComponent.AdjustThreat(15);
            // UIManager.Instance.ShowYesNoConfirmation("Demonic Structure Reported",
            //     $"Your demonic structure {structureToReport.name} has been reported by {goapNode.actor.name}! They can now attack this structure!", 
            //     onClickNoAction: goapNode.actor.CenterOnCharacter, yesBtnText: "OK", noBtnText: $"Jump to {goapNode.actor.name}", 
            //     showCover:true, pauseAndResume: true);
            // PlayerUI.Instance.ShowGeneralConfirmation("Demonic Structure Reported", "Your demonic structure " + structureToReport.name + " has been reported! They can now attack this structure!");
        }
    }
    #endregion
}