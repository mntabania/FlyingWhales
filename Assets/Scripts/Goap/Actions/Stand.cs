using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;  
using Traits;

public class Stand : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public Stand() : base(INTERACTION_TYPE.STAND) {
        actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
        actionIconString = GoapActionStateDB.No_Icon;
        
        shouldAddLogs = false;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        logTags = new[] {LOG_TAG.Needs};
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Stand Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        return 4;
    }
    public override void PopulateNearbyLocation(List<LocationGridTile> gridTiles, ActualGoapNode goapNode) {
        if (goapNode.actor is Summon && goapNode.actor.homeStructure != null && goapNode.actor.homeStructure == goapNode.actor.currentStructure) {
            //This might be performance heavy because it returns a new list every time, that is why I switched it to all tiles instead of unoccupied tiles
            //return goapNode.actor.homeStructure.unoccupiedTiles.ToList();
            gridTiles.AddRange(goapNode.actor.homeStructure.passableTiles);
        }
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
}