using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Repair : GoapAction {

    public Repair() : base(INTERACTION_TYPE.REPAIR) {
        //actionLocationType = ACTION_LOCATION_TYPE.ON_TARGET;
        actionIconString = GoapActionStateDB.Work_Icon;
        
        canBeAdvertisedEvenIfActorIsUnavailable = true;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.ELEMENTAL, RACE.KOBOLD };
        validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.MORNING, TIME_IN_WORDS.LUNCH_TIME, TIME_IN_WORDS.AFTERNOON, TIME_IN_WORDS.EARLY_NIGHT };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Wood Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasSupply);
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Burnt", false, GOAP_EFFECT_TARGET.TARGET));
    }
    //public override List<Precondition> GetPreconditions(IPointOfInterest poiTarget, object[] otherData) {
    //    List <Precondition> p = new List<Precondition>(base.GetPreconditions(poiTarget, otherData));
    //    TileObject tileObj = poiTarget as TileObject;
    //    TileObjectData data = TileObjectDB.GetTileObjectData(tileObj.tileObjectType);
    //    int craftCost = (int)(data.constructionCost * 0.5f);
    //    p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_WOOD, craftCost.ToString(), true, GOAP_EFFECT_TARGET.ACTOR), HasSupply));
    //    return p;
    //}
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Repair Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        string stateName = "Target Missing";
        bool defaultTargetMissing = IsRepairTargetMissing(node);
        GoapActionInvalidity goapActionInvalidity = new GoapActionInvalidity(defaultTargetMissing, stateName);
        //if (defaultTargetMissing == false) {
        //    //check the target's traits, if any of them can make this action invalid
        //    for (int i = 0; i < poiTarget.traitContainer.allTraits.Count; i++) {
        //        Trait trait = poiTarget.traitContainer.allTraits[i];
        //        if (trait.TryStopAction(goapType, actor, poiTarget, ref goapActionInvalidity)) {
        //            break; //a trait made this action invalid, stop loop
        //        }
        //    }
        //}
        return goapActionInvalidity;
    }
    private bool IsRepairTargetMissing(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        if (poiTarget.gridTileLocation == null || actor.currentRegion != poiTarget.currentRegion) {
            return true;
        }
        if (actionLocationType == ACTION_LOCATION_TYPE.NEAR_TARGET) {
            //if the action type is NEAR_TARGET, then check if the actor is near the target, if not, this action is invalid.
            if (actor.gridTileLocation != poiTarget.gridTileLocation && actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation) == false) {
                return true;
            }
        } else if (actionLocationType == ACTION_LOCATION_TYPE.NEAR_OTHER_TARGET) {
            //if the action type is NEAR_TARGET, then check if the actor is near the target, if not, this action is invalid.
            if (actor.gridTileLocation != node.targetTile && actor.gridTileLocation.IsNeighbour(node.targetTile) == false) {
                return true;
            }
        }
        return false;
    }
    public override void OnStopWhileStarted(ActualGoapNode node) {
        base.OnStopWhileStarted(node);
        Character actor = node.actor;
        actor.UncarryPOI();
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        actor.UncarryPOI();
    }
    #endregion

    #region State Effects
    public void PreRepairSuccess(ActualGoapNode goapNode) {
        //goapNode.descriptionLog.AddToFillers(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        TileObject tileObj = goapNode.poiTarget as TileObject;
        TileObjectData data = TileObjectDB.GetTileObjectData(tileObj.tileObjectType);
        int cost = (int) (data.constructionCost * 0.5f);
        if (goapNode.actor.ownParty.carriedPOI != null) {
            ResourcePile carriedPile = goapNode.actor.ownParty.carriedPOI as ResourcePile;
            carriedPile.AdjustResourceInPile(-cost);
            tileObj.AdjustResource(RESOURCE.WOOD, cost);
        }
    }
    public void PerTickRepairSuccess(ActualGoapNode goapNode) {
        goapNode.poiTarget.AdjustHP(20, ELEMENTAL_TYPE.Normal);
    }
    public void AfterRepairSuccess(ActualGoapNode goapNode) {
        goapNode.poiTarget.traitContainer.RemoveTrait(goapNode.poiTarget, "Burnt");
        goapNode.poiTarget.traitContainer.RemoveTrait(goapNode.poiTarget, "Damaged");

        TileObject tileObj = goapNode.poiTarget as TileObject;
        TileObjectData data = TileObjectDB.GetTileObjectData(tileObj.tileObjectType);
        int cost = (int) (data.constructionCost * 0.5f);
        tileObj.AdjustResource(RESOURCE.WOOD, -cost);
        //goapNode.actor.AdjustSupply((int) (data.constructionCost * 0.5f));

    }
    #endregion

    #region Preconditions
    private bool HasSupply(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        TileObject tileObj = poiTarget as TileObject;
        TileObjectData data = TileObjectDB.GetTileObjectData(tileObj.tileObjectType);
        int craftCost = (int)(data.constructionCost * 0.5f);
        if (poiTarget.HasResourceAmount(RESOURCE.WOOD, craftCost)) {
            return true;
        }
        if (actor.ownParty.isCarryingAnyPOI && actor.ownParty.carriedPOI is WoodPile) {
            //ResourcePile carriedPile = actor.ownParty.carriedPOI as ResourcePile;
            //return carriedPile.resourceInPile >= craftCost;
            return true;
        }
        return false;
        //return actor.supply >= craftCost;
    }
    #endregion

}

public class RepairData : GoapActionData {
    public RepairData() : base(INTERACTION_TYPE.REPAIR) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.ELEMENTAL, RACE.KOBOLD };
        //requirementAction = Requirement;
    }

    //private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
    //    return actor == poiTarget;
    //}
}
