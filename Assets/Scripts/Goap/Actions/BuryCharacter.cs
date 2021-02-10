﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;  
using Traits;
using UtilityScripts;

public class BuryCharacter : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public BuryCharacter() : base(INTERACTION_TYPE.BURY_CHARACTER) {
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION_B;
        actionIconString = GoapActionStateDB.Bury_Icon;
        canBeAdvertisedEvenIfTargetIsUnavailable = true;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Work};
    }

    #region Overrides
    public override LocationStructure GetTargetStructure(ActualGoapNode node) {
        Character actor = node.actor;
        OtherData[] otherData = node.otherData;
        if(node.associatedJobType == JOB_TYPE.BURY_IN_ACTIVE_PARTY) {
            return actor.currentStructure;
        }
        if (otherData != null && otherData.Length >= 1 && otherData[0].obj is LocationStructure) {
            return otherData[0].obj as LocationStructure;
        } else {
            return actor.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.CEMETERY) ?? actor.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
        }
    }
    public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode) {
        if (goapNode.associatedJobType == JOB_TYPE.BURY_IN_ACTIVE_PARTY) {
            Character actor = goapNode.actor;
            if (actor.limiterComponent.canMove && !actor.movementComponent.isStationary) {
                List<LocationGridTile> choices = actor.gridTileLocation.GetTilesInRadius(3, includeImpassable: false);
                if (choices != null && choices.Count > 0) {
                    return choices[UtilityScripts.Utilities.Rng.Next(0, choices.Count)];
                } else {
                    return actor.gridTileLocation;
                }
            } else {
                return actor.gridTileLocation;
            }
        }
        if (goapNode.otherData != null && goapNode.otherData.Length == 2 && goapNode.otherData[1].obj is LocationGridTile) {
            return goapNode.otherData[1].obj as LocationGridTile;
        } else {
            LocationStructure targetStructure = GetTargetStructure(goapNode);
            if (targetStructure.structureType == STRUCTURE_TYPE.WILDERNESS) {
                if(goapNode.actor.homeSettlement != null) {
                    List<Area> surroundingAreas = ObjectPoolManager.Instance.CreateNewAreaList();
                    goapNode.actor.homeSettlement.PopulateSurroundingAreas(surroundingAreas);
                    surroundingAreas = CollectionUtilities.Shuffle(surroundingAreas);
                    List<LocationGridTile> validTiles = null;
                    for (int i = 0; i < surroundingAreas.Count; i++) {
                        Area surroundingArea = surroundingAreas[i];
                        for (int j = 0; j < surroundingArea.gridTileComponent.gridTiles.Count; j++) {
                            LocationGridTile tileInSurroundingArea = surroundingArea.gridTileComponent.gridTiles[j];
                            if (!tileInSurroundingArea.isOccupied && tileInSurroundingArea.IsNextToSettlement(goapNode.actor.homeSettlement) && tileInSurroundingArea.structure is Wilderness) {
                                if (validTiles == null) { validTiles = new List<LocationGridTile>(); }
                                validTiles.Add(tileInSurroundingArea);
                            }
                        }
                        if (validTiles != null) { break; }
                    }
                    ObjectPoolManager.Instance.ReturnAreaListToPool(surroundingAreas);
                    if (validTiles == null) {
                        //fallback
                        validTiles = targetStructure.unoccupiedTiles.Where(tile => tile.IsNextToSettlement(goapNode.actor.homeSettlement)).ToList(); 
                    }
                    return CollectionUtilities.GetRandomElement(validTiles);
                } else if (goapNode.poiTarget.gridTileLocation != null) {
                    return goapNode.poiTarget.gridTileLocation.GetNearestUnoccupiedTileFromThisWithStructure(targetStructure.structureType);
                } else if (goapNode.actor.gridTileLocation != null) {
                    return goapNode.actor.gridTileLocation.GetNearestUnoccupiedTileFromThisWithStructure(targetStructure.structureType);
                }
            } else if (targetStructure.structureType == STRUCTURE_TYPE.CEMETERY) {
                List<LocationGridTile> validTiles = targetStructure.unoccupiedTiles.Where(tile => tile.groundType != LocationGridTile.Ground_Type.Ruined_Stone).ToList();
                if (validTiles.Count <= 0) {
                    validTiles = new List<LocationGridTile>(targetStructure.unoccupiedTiles);
                }
                return CollectionUtilities.GetRandomElement(validTiles);
            }
        }
        return null; //allow normal logic to pick target tile
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        SetPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_POI, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.TARGET }, IsCarried);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Bury Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        return 1;
    }
    public override void OnStopWhileStarted(ActualGoapNode node) {
        base.OnStopWhileStarted(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        if (poiTarget is Character targetCharacter) {
            actor.UncarryPOI(targetCharacter, addToLocation: false);
            // targetCharacter.SetCurrentStructureLocation(targetCharacter.gridTileLocation.structure, false);    
        }
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        string stateName = "Target Missing";
        GoapActionInvalidity goapActionInvalidity = new GoapActionInvalidity(false, stateName);
        //bury cannot be invalid because all cases are handled by the requirements of the action
        return goapActionInvalidity;
    }
    #endregion

    #region State Effects
    public void PreBurySuccess(ActualGoapNode goapNode) { }
    public void AfterBurySuccess(ActualGoapNode goapNode) {
        //if (parentPlan.job != null) {
        //    parentPlan.job.SetCannotCancelJob(true);
        //}
        //SetCannotCancelAction(true);

        Character targetCharacter = goapNode.poiTarget as Character;
        //**After Effect 1**: Remove Target from Actor's Party.
        goapNode.actor.UncarryPOI(goapNode.poiTarget, addToLocation: false);
        //**After Effect 2**: Place a Tombstone tile object in adjacent unoccupied tile, link it with Target.
        LocationGridTile chosenLocation = goapNode.actor.gridTileLocation;
        if (chosenLocation.isOccupied) {
            List<LocationGridTile> choices = goapNode.actor.gridTileLocation.UnoccupiedNeighbours.Where(x => x.structure == goapNode.actor.currentStructure).ToList();
            if (choices.Count > 0) {
                chosenLocation = choices[Random.Range(0, choices.Count)];
            }
        }
        Tombstone tombstone = new Tombstone();
        tombstone.SetCharacter(targetCharacter);
        goapNode.actor.currentStructure.AddPOI(tombstone, chosenLocation);

        //Note: Added this because it is stated in the Bury Job document that all other bury jobs must be cancelled instantaneously when the character is buried
        //This might cause some problems because it is a bad form to call cancelling jobs whenever an action of the same type is being done
        //The other solution is to just let the other systems handle the bury job that is still lingering. It might not be instantaneous, but at least it is not prone to errors
        targetCharacter.ForceCancelAllJobsTargetingThisCharacter(JOB_TYPE.BURY);
    }
    #endregion

    #region Preconditions
    private bool IsCarried(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        // Character target = poiTarget as Character;
        // return target.currentParty == actor.currentParty;
        return actor.IsPOICarriedOrInInventory(poiTarget);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (poiTarget is Character targetCharacter) {
                //target character must be dead
                if (!targetCharacter.isDead) {
                    return false;
                }
                //check that the charcater has been buried (has a grave)
                if (targetCharacter.grave != null) {
                    return false;
                }
                if (targetCharacter.numOfActionsBeingPerformedOnThis > 0) {
                    return false;
                }
                if (targetCharacter.marker == null) {
                    return false;
                }
                if (otherData != null && otherData.Length >= 1 && otherData[0].obj is LocationStructure) {
                    //if structure is provided, do not check for cemetery
                    return true;
                } else {
                    return actor.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.CEMETERY) != null;
                }   
            }
        }
        return false;
    }
    #endregion
}