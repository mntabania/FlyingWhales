﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class FeelingConcerned : GoapAction {

    public FeelingConcerned() : base(INTERACTION_TYPE.FEELING_CONCERNED) {
        actionIconString = GoapActionStateDB.Flirt_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        doesNotStopTargetCharacter = true;
        canBeAdvertisedEvenIfTargetIsUnavailable = true;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Social};
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Concerned Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        return 1;
    }
    #endregion

    #region State Effects
    public void PreConcernedSuccess() {
        
    }
    public void AfterConcernedSuccess() {
       
    }
    #endregion   
}