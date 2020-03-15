using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class CreateHealingPotion : GoapAction {

    public CreateHealingPotion() : base(INTERACTION_TYPE.CREATE_HEALING_POTION) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.Work_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Herb Plant", false, GOAP_EFFECT_TARGET.ACTOR), HasHerbPlant);
        AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Water Flask", false, GOAP_EFFECT_TARGET.ACTOR), HasWaterFlask);
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Healing Potion", false, GOAP_EFFECT_TARGET.ACTOR));
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
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}:";
        int cost = 250;
        costLog += $" +{cost}(Initial)";
        actor.logComponent.AppendCostLog(costLog);
        return cost;
    }
    #endregion

    #region State Effects
    public void AfterCreateSuccess(ActualGoapNode goapNode) {
        Character actor = goapNode.actor;
        if(actor.HasItem(TILE_OBJECT_TYPE.HERB_PLANT) && actor.HasItem(TILE_OBJECT_TYPE.WATER_FLASK)) {
            actor.ObtainItem(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.HEALING_POTION));
            actor.UnobtainItem(TILE_OBJECT_TYPE.HERB_PLANT);
            actor.UnobtainItem(TILE_OBJECT_TYPE.WATER_FLASK);
        } else {
            actor.logComponent.PrintLogErrorIfActive(actor.name + " is trying to create a Healing Potion but lacks requirements");
        }
    }
    #endregion

    #region Preconditions
    private bool HasHerbPlant (Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor.HasItem("Herb Plant");
    }
    private bool HasWaterFlask(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor.HasItem("Water Flask");
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