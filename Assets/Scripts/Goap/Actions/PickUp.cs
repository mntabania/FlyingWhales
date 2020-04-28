using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class PickUp : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public PickUp() : base(INTERACTION_TYPE.PICK_UP) {
        actionIconString = GoapActionStateDB.Explore_Icon;
        //actionLocationType = ACTION_LOCATION_TYPE.ON_TARGET;
        
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.DEMON };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        //SpecialToken token = poiTarget as SpecialToken;
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = poiTarget, targetPOI = actor });
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = token.specialTokenType.ToString(), targetPOI = actor });
        AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_POI, GOAP_EFFECT_TARGET.TARGET));
        AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_POI, GOAP_EFFECT_TARGET.ACTOR));
    }
    protected override List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, object[] otherData) {
        List <GoapEffect> ee = base.GetExpectedEffects(actor, target, otherData);
        TileObject item = target as TileObject;
        ee.Add(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_POI, conditionKey = item.name, target = GOAP_EFFECT_TARGET.TARGET });
        ee.Add(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_POI, conditionKey = item.name, target = GOAP_EFFECT_TARGET.ACTOR });
        return ee;
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Take Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}:";
        int cost = UtilityScripts.Utilities.Rng.Next(80, 121);
        costLog += $" +{cost}(RNG)";
        actor.logComponent.AppendCostLog(costLog);
        return cost;
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        if (node.poiTarget is TileObject tileObject) {
            if (tileObject.characterOwner != null && tileObject.characterOwner != node.actor) {
                return REACTABLE_EFFECT.Negative;
            }
        }
        return REACTABLE_EFFECT.Neutral;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            TileObject item = poiTarget as TileObject;
            return poiTarget.gridTileLocation != null && !actor.HasItem(item);
        }
        return false;
        
    }
    #endregion

    #region State Effects
    public void PreTakeSuccess(ActualGoapNode goapNode) {
        //GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
        goapNode.descriptionLog.AddToFillers(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.ITEM_1);
        //goapNode.descriptionLog.AddToFillers(goapNode.targetStructure.location, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterTakeSuccess(ActualGoapNode goapNode) {
        goapNode.actor.PickUpItem(goapNode.poiTarget as TileObject);
    }
    #endregion
}

public class PickItemData : GoapActionData {
    public PickItemData() : base(INTERACTION_TYPE.PICK_UP) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        TileObject item = poiTarget as TileObject;
        return poiTarget.gridTileLocation != null && !actor.HasItem(item);
    }
}
