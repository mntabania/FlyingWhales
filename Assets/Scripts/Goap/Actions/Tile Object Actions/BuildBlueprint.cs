using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Logs;
using UnityEngine;

public class BuildBlueprint : GoapAction {

    public BuildBlueprint() : base(INTERACTION_TYPE.BUILD_BLUEPRINT) {
        actionIconString = GoapActionStateDB.Build_Icon;
        showNotification = true;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.MORNING, TIME_IN_WORDS.LUNCH_TIME, TIME_IN_WORDS.AFTERNOON, TIME_IN_WORDS.EARLY_NIGHT };
        logTags = new[] {LOG_TAG.Work};
    }

    #region Overrides
    // protected override void ConstructBasePreconditionsAndEffects() {
    //     AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Wood Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasSupply);
    // }
    public override List<Precondition> GetPreconditions(Character actor, IPointOfInterest target, OtherData[] otherData) {
        if(target is GenericTileObject genericTileObject) {
            if (genericTileObject.blueprintOnTile != null) {
                List<Precondition> p = new List<Precondition>();
                switch (genericTileObject.blueprintOnTile.thinWallResource) {
                    case RESOURCE.STONE:
                        p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Stone Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasResource));
                        break;
                    case RESOURCE.WOOD:
                        p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Wood Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasResource));
                        break;
                    case RESOURCE.METAL:
                        p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Metal Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasResource));
                        break;
                    default:
                        p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Wood Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasResource));
                        break;
                }
                return p;
            }
        }
        return base.GetPreconditions(actor, target, otherData);
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Build Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override void AddFillersToLog(ref Log log, ActualGoapNode goapNode) {
        base.AddFillersToLog(ref log, goapNode);
        if (goapNode.poiTarget is GenericTileObject genericTileObject && genericTileObject.blueprintOnTile != null) {
            log.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(genericTileObject.blueprintOnTile.structureType.ToString()), LOG_IDENTIFIER.STRING_1);
        }
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

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation == null) {
                return false;
            }
            if (poiTarget is GenericTileObject genericTileObject) {
                if (genericTileObject.blueprintOnTile == null) {
                    return false;
                }  
            } else {
                return false;
            }
            //TODO:
            // StructureTileObject structure = poiTarget as StructureTileObject;
            // return structure.spot.hasBlueprint;
            return actor.homeSettlement != null;
        }
        return false;
    }
    #endregion

    #region Preconditions
    private bool HasResource(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType) {
        if (poiTarget is GenericTileObject genericTileObject && genericTileObject.blueprintOnTile != null) {
            if (poiTarget.HasResourceAmount(genericTileObject.blueprintOnTile.thinWallResource, genericTileObject.blueprintOnTile.craftCost)) {
                return true;
            }
            //return actor.ownParty.isCarryingAnyPOI && actor.ownParty.carriedPOI is ResourcePile;
            if (actor.carryComponent.isCarryingAnyPOI && actor.carryComponent.carriedPOI is ResourcePile resourcePile) {
                return resourcePile.providedResource == genericTileObject.blueprintOnTile.thinWallResource && resourcePile.resourceInPile >= genericTileObject.blueprintOnTile.craftCost;
            }    
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreBuildSuccess(ActualGoapNode goapNode) {
        if (goapNode.actor.carryComponent.carriedPOI is ResourcePile carriedPile && goapNode.poiTarget is GenericTileObject genericTileObject && genericTileObject.blueprintOnTile != null) {
            carriedPile.AdjustResourceInPile(-genericTileObject.blueprintOnTile.craftCost);
            goapNode.poiTarget.AdjustResource(genericTileObject.blueprintOnTile.thinWallResource, genericTileObject.blueprintOnTile.craftCost);
            goapNode.descriptionLog.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(genericTileObject.blueprintOnTile.structureType.ToString()), LOG_IDENTIFIER.STRING_1);
        }
    }
    public void AfterBuildSuccess(ActualGoapNode goapNode) {
        if (goapNode.poiTarget is GenericTileObject genericTileObject) {
            genericTileObject.BuildBlueprint(goapNode.actor.homeSettlement);
        }
        //PlayerUI.Instance.ShowGeneralConfirmation("New Structure", $"A new {structure.name} has been built at {spot.gridTileLocation.structure.location.name}");
    }
    #endregion
}

