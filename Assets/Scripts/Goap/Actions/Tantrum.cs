using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;  
using Traits;

public class Tantrum : GoapAction {

    private string reason;

    public Tantrum() : base(INTERACTION_TYPE.TANTRUM) {
        //showNotification = false;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.Anger_Icon;
        //
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Berserked", target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Tantrum Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        //**Cost**: randomize between 3-10
        return UtilityScripts.Utilities.Rng.Next(3, 11);
    }
    #endregion

    #region Effects
    public void PreTantrumSuccess(ActualGoapNode goapNode) {
        goapNode.descriptionLog.AddToFillers(null, (string)goapNode.otherData[0], LOG_IDENTIFIER.STRING_1);
    }
    public void AfterTantrumSuccess(ActualGoapNode goapNode) {
        goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Berserked");
    }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return actor == poiTarget;
        }
        return false;
    }
    #endregion
}

public class TantrumData : GoapActionData {
    public TantrumData() : base(INTERACTION_TYPE.TANTRUM) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor == poiTarget;
    }
}