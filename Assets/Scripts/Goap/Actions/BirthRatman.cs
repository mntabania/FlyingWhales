
using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using Inner_Maps;

public class BirthRatman : GoapAction {

    public BirthRatman() : base(INTERACTION_TYPE.BIRTH_RATMAN) {
        actionIconString = GoapActionStateDB.Happy_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.RATMAN, };
        logTags = new[] {LOG_TAG.Life_Changes};
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Birth Success", goapNode);
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
            return actor.gridTileLocation != null;
        }
        return false;
    }
#endregion

#region State Effects
    public void AfterBirthSuccess(ActualGoapNode goapNode) {
        Character actor = goapNode.actor;
        if(actor.gridTileLocation != null) {
            Character newCharacter = CharacterManager.Instance.CreateNewCharacter("Ratman", RACE.RATMAN, actor.gender, actor.faction, actor.homeSettlement, actor.homeRegion, actor.homeStructure);
            newCharacter.CreateMarker();
            newCharacter.InitialCharacterPlacement(actor.gridTileLocation);
        }
    }
#endregion

}