using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;
using Traits;
using Locations.Features;

public class TakeShelter : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.INDIRECT; } }

    public TakeShelter() : base(INTERACTION_TYPE.TAKE_SHELTER) {
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION;
        actionIconString = GoapActionStateDB.Cowering_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        doesNotStopTargetCharacter = true;
    }

    #region Overrides
    public override LocationStructure GetTargetStructure(ActualGoapNode node) {
        object[] otherData = node.otherData;
        if (otherData != null && otherData.Length == 1) {
            if (otherData[0] is LocationStructure) {
                return otherData[0] as LocationStructure;
            } 
        }
        return null;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        object[] otherData = node.otherData;
        if (otherData != null && otherData.Length >= 1) {
            if (otherData[0] is LocationStructure) {
                LocationStructure structure = otherData[0] as LocationStructure; 
                log.AddToFillers(structure, structure.GetNameRelativeTo(node.actor), LOG_IDENTIFIER.LANDMARK_1);
            } 
        }
        string traits = string.Empty;
        if (node.actor.traitContainer.HasTrait("Freezing")) {
            traits += "Freezing"; 
        }
        if (node.actor.traitContainer.HasTrait("Overheating")) {
            if(traits != string.Empty) {
                traits += " and ";
            }
            traits += "Overheating";
        }
        if (traits != string.Empty) {
            log.AddToFillers(null, traits, LOG_IDENTIFIER.STRING_1);
        }

    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Take Shelter Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
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

    #region State Effects
    public void AfterTakeShelterSuccess(ActualGoapNode goapNode) {
        bool shouldSetShelter = goapNode.actor.gridTileLocation != null && goapNode.actor.gridTileLocation.collectionOwner.isPartOfParentRegionMap && 
            (goapNode.actor.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.featureComponent.HasFeature(TileFeatureDB.Blizzard_Feature) 
            || goapNode.actor.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.featureComponent.HasFeature(TileFeatureDB.Heat_Wave_Feature));
        if (shouldSetShelter) {
            if (goapNode.actor.traitContainer.HasTrait("Freezing")) {
                Freezing freezing = goapNode.actor.traitContainer.GetNormalTrait<Freezing>("Freezing");
                freezing.SetCurrentShelterStructure(goapNode.targetStructure);
            }
            if (goapNode.actor.traitContainer.HasTrait("Overheating")) {
                Overheating overheating = goapNode.actor.traitContainer.GetNormalTrait<Overheating>("Overheating");
                overheating.SetCurrentShelterStructure(goapNode.targetStructure);
            }
            goapNode.actor.trapStructure.SetForcedStructure(goapNode.targetStructure);
        }
    }
    #endregion
}
