﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class LaughAt : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.INDIRECT; } }

    public LaughAt() : base(INTERACTION_TYPE.LAUGH_AT) {
        actionIconString = GoapActionStateDB.Mock_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        doesNotStopTargetCharacter = true;
        canBeAdvertisedEvenIfTargetIsUnavailable = true;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER, POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Social};
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Laugh Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        return UtilityScripts.Utilities.Rng.Next(40, 61);
    }
    #endregion

    #region State Effects
    //public void PerTickLaughSuccess(ActualGoapNode goapNode) {
    //    goapNode.actor.needsComponent.AdjustHappiness(500);
    //}
    public void AfterLaughSuccess(ActualGoapNode goapNode) {
        if (!goapNode.poiTarget.traitContainer.HasTrait("Unconscious")) {
            goapNode.poiTarget.traitContainer.AddTrait(goapNode.poiTarget, "Ashamed");
        }
    }
    #endregion   
}