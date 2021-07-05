using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class CreateAntidote : GoapAction {

    public CreateAntidote() : base(INTERACTION_TYPE.CREATE_ANTIDOTE) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.Work_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES };
        logTags = new[] {LOG_TAG.Work};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        SetPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Poison Flask", false, GOAP_EFFECT_TARGET.ACTOR), HasPoisonFlask);
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Antidote", false, GOAP_EFFECT_TARGET.ACTOR));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Create Success", goapNode);
    }
    //public override void AddFillersToLog(Log log, ActualGoapNode node) {
    //    base.AddFillersToLog(log, node);
    //    TileObject obj = node.poiTarget as TileObject;
    //    log.AddToFillers(null, UtilityScripts.Utilities.GetArticleForWord(obj.tileObjectType.ToString()), LOG_IDENTIFIER.STRING_1);
    //}
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        int cost = 250;
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}:";
        costLog += $" +{cost}(Initial)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return cost;
    }
#endregion

#region State Effects
    public void AfterCreateSuccess(ActualGoapNode goapNode) {
        Character actor = goapNode.actor;
        if(actor.HasItem(TILE_OBJECT_TYPE.POISON_FLASK)) {
            actor.ObtainItem(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.ANTIDOTE));
            actor.UnobtainItem(TILE_OBJECT_TYPE.POISON_FLASK);
        } else {
#if DEBUG_LOG
            actor.logComponent.PrintLogErrorIfActive(actor.name + " is trying to create a Healing Potion but lacks requirements");
#endif
        }
    }
#endregion

#region Preconditions
    private bool HasPoisonFlask(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        return actor.HasItem("Poison Flask");
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