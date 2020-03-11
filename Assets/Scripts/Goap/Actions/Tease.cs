using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Tease : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.INDIRECT; } }

    public Tease() : base(INTERACTION_TYPE.TEASE) {
        actionIconString = GoapActionStateDB.Entertain_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        doesNotStopTargetCharacter = true;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.ELEMENTAL, RACE.KOBOLD };
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Tease Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest poiTarget, JobQueueItem job,
        object[] otherData) {
        Character targetCharacter = poiTarget as Character;
        if (actor.relationshipContainer.IsFriendsWith(targetCharacter)) {
            return UtilityScripts.Utilities.rng.Next(40, 61);
        } else {
            return UtilityScripts.Utilities.rng.Next(50, 71);
        }
    }
    #endregion

    #region State Effects
    public void PerTickTeaseSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustHappiness(5f);
    }
    #endregion   
}

public class TeaseData : GoapActionData {
    public TeaseData() : base(INTERACTION_TYPE.TEASE) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.ELEMENTAL, RACE.KOBOLD };
    }
}

