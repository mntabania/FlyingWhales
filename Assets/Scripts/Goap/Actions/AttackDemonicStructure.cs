using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;

public class AttackDemonicStructure : GoapAction {

	public AttackDemonicStructure() : base(INTERACTION_TYPE.ATTACK_DEMONIC_STRUCTURE) {
		actionIconString = GoapActionStateDB.Hostile_Icon;
		doesNotStopTargetCharacter = true;
		advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
		racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.ANGEL };
	}

	#region Overrides
	public override void Perform(ActualGoapNode goapNode) {
		base.Perform(goapNode);
		SetState("Attack Success", goapNode);
	}
	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
		string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
		actor.logComponent.AppendCostLog(costLog);
		return 10;
	}
	public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode) {
		object[] otherData = goapNode.otherData;
		if (otherData != null) {
			if (otherData.Length == 1 && otherData[0] is LocationGridTile) { // && otherData[1] is LocationGridTile
				return otherData[0] as LocationGridTile;
			}
		}
		return base.GetTargetTileToGoTo(goapNode);
	}
	public override IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode) {
		//This must return null so that the GetTargetTileToGoTo will be triggered
		return null;
	}
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        LocationStructure targetStructure = node.targetStructure;
        log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER); //Target character is only the identifier but it doesn't mean that this is a character, it can be item, etc.
        log.AddToFillers(targetStructure, actor.behaviourComponent.attackDemonicStructureTarget.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    #endregion
}