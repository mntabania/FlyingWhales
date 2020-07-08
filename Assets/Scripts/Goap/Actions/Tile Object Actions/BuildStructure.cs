using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;

public class BuildStructure : GoapAction {

    public BuildStructure() : base(INTERACTION_TYPE.BUILD_STRUCTURE) {
        actionIconString = GoapActionStateDB.Build_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.MORNING, TIME_IN_WORDS.LUNCH_TIME, TIME_IN_WORDS.AFTERNOON, TIME_IN_WORDS.EARLY_NIGHT };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Wood Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasSupply);
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Build Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode goapNode) {
        base.AddFillersToLog(log, goapNode);
        StructureTileObject target = goapNode.poiTarget as StructureTileObject;
        // log.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(target.spot.blueprintType.ToString()), LOG_IDENTIFIER.STRING_1);
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
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation == null) {
                return false;
            }
            //TODO:
            // StructureTileObject structure = poiTarget as StructureTileObject;
            // return structure.spot.hasBlueprint;
        }
        return false;
    }
    #endregion

    #region Preconditions
    private bool HasSupply(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        if (poiTarget.HasResourceAmount(RESOURCE.WOOD, 50)) {
            return true;
        }
        //return actor.ownParty.isCarryingAnyPOI && actor.ownParty.carriedPOI is ResourcePile;
        if (actor.carryComponent.isCarryingAnyPOI && actor.carryComponent.carriedPOI is WoodPile) {
            //ResourcePile carriedPile = actor.ownParty.carriedPOI as ResourcePile;
            //return carriedPile.resourceInPile >= 50;
            return true;
        }
        return false;
        //return actor.HasItem()
    }
    #endregion

    #region State Effects
    public void PreBuildSuccess(ActualGoapNode goapNode) {
        StructureTileObject target = goapNode.poiTarget as StructureTileObject;
        if (goapNode.actor.carryComponent.carriedPOI != null) {
            ResourcePile carriedPile = goapNode.actor.carryComponent.carriedPOI as ResourcePile;
            int cost = TileObjectDB.GetTileObjectData((goapNode.poiTarget as TileObject).tileObjectType).constructionCost;
            carriedPile.AdjustResourceInPile(-50);
            goapNode.poiTarget.AdjustResource(RESOURCE.WOOD, 50);
        }
        // goapNode.descriptionLog.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(target.spot.blueprintType.ToString()), LOG_IDENTIFIER.STRING_1);
    }
    public void AfterBuildSuccess(ActualGoapNode goapNode) {
        StructureTileObject spot = goapNode.poiTarget as StructureTileObject;
        // LocationStructure structure = spot.BuildBlueprint(goapNode.actor.homeSettlement);
        // goapNode.poiTarget.AdjustResource(RESOURCE.WOOD, -50);

        //PlayerUI.Instance.ShowGeneralConfirmation("New Structure", $"A new {structure.name} has been built at {spot.gridTileLocation.structure.location.name}");
    }
    #endregion
}

