using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class Roam : GoapAction {

	public Roam() : base(INTERACTION_TYPE.ROAM) {
		// actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION;
		actionIconString = GoapActionStateDB.No_Icon;
		doesNotStopTargetCharacter = true;
		advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
		racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, 
			RACE.SPIDER, RACE.DRAGON, RACE.GOLEM, RACE.DEMON, RACE.ELEMENTAL, RACE.KOBOLD, RACE.MIMIC };
	}

	#region Overrides
	protected override void ConstructBasePreconditionsAndEffects() {
		// AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.IN_VISION, string.Empty, false, GOAP_EFFECT_TARGET.TARGET ));
	}
	public override void Perform(ActualGoapNode goapNode) {
		base.Perform(goapNode);
		SetState("Roam Success", goapNode);
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
	#endregion

	//#region State Effects
	//public void AfterSpookedSuccess() {
	//    //if (parentPlan != null && parentPlan.job != null) {
	//    //    parentPlan.job.SetCannotOverrideJob(true);//Carry should not be overrideable if the character is actually already carrying another character.
	//    //}
	//    Character target = poiTarget as Character;
	//    actor.ownParty.AddCharacter(target);
	//}
	//#endregion
}