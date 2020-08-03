﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Pathfinding;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;
using UtilityScripts;

public class MovementComponent {
    public Character owner { get; private set; }

    public bool isRunning { get; private set; }
    public bool noRunExceptCombat { get; private set; }
    public bool noRunWithoutException { get; private set; }
    public int useRunSpeed { get; private set; }
    public float speedModifier { get; private set; }
    public float walkSpeedModifier { get; private set; }
    public float runSpeedModifier { get; private set; }
    public bool hasMovedOnCorruption { get; private set; }
    public bool isStationary { get; private set; }
    public ABPath currentDigPath { get; private set; }

    private int _enableDiggingCounter;

    #region getters
    public float walkSpeed => owner.raceSetting.walkSpeed + (owner.raceSetting.walkSpeed * walkSpeedModifier);
    public float runSpeed => owner.raceSetting.runSpeed + (owner.raceSetting.runSpeed * runSpeedModifier);
    public bool enableDigging => _enableDiggingCounter > 0;
    #endregion

    public MovementComponent(Character owner) {
        this.owner = owner;
    }

    public void UpdateSpeed() {
        if (owner.marker) {
            SetMovementState();
            owner.marker.pathfindingAI.speed = GetSpeed();
            Messenger.Broadcast(Signals.UPDATE_MOVEMENT_STATE, owner);
        }
        //Debug.Log("Updated speed of " + character.name + ". New speed is: " + pathfindingAI.speed.ToString());
    }
    public void SetIsRunning(bool state) {
        isRunning = state;
    }

    public void SetNoRunExceptCombat(bool state) {
        noRunExceptCombat = state;
    }
    public void SetNoRunWithoutException(bool state) {
        noRunWithoutException = state;
    }
    public void AdjustSpeedModifier(float amount) {
        speedModifier += amount;
        UpdateSpeed();
    }
    public void AdjustWalkSpeedModifier(float amount) {
        walkSpeedModifier += amount;
    }
    public void AdjustRunSpeedModifier(float amount) {
        runSpeedModifier += amount;
    }
    private float GetSpeed() {
        float speed = runSpeed;
        if (!isRunning) {
            speed = walkSpeed;
        }
        speed += (speed * speedModifier);
        if (speed <= 0f) {
            speed = 0.5f;
        }
        if (owner.marker) {
            speed *= owner.marker.progressionSpeedMultiplier;
        } else {
            throw new System.Exception("Trying to get speed for " + owner.name + " without a marker, this canot happen!");
        }
        return speed;
    }

    //Sets if character should walk or run
    private void SetMovementState() {
        SetIsRunning(false);
        if (noRunWithoutException || (noRunExceptCombat && !owner.combatComponent.isInCombat)) {
            return;
        }
        if (useRunSpeed > 0) {
            SetIsRunning(true);
            return;
        } else {
            //If character is in combat/fleeing or character has urgent recovery, douse fire, remove status, neutralize danger, apprehend, report corrupted structure, restrain jobs
            //Set running to TRUE
            if (owner.combatComponent.isInActualCombat) {
                SetIsRunning(true);
                return;
            } else if (owner.currentActionNode != null) {
                if (owner.currentActionNode.associatedJobType == JOB_TYPE.ENERGY_RECOVERY_URGENT
                    || owner.currentActionNode.associatedJobType == JOB_TYPE.FULLNESS_RECOVERY_URGENT
                    || owner.currentActionNode.associatedJobType == JOB_TYPE.DOUSE_FIRE
                    || owner.currentActionNode.associatedJobType == JOB_TYPE.REMOVE_STATUS
                    || owner.currentActionNode.associatedJobType == JOB_TYPE.NEUTRALIZE_DANGER
                    || owner.currentActionNode.associatedJobType == JOB_TYPE.APPREHEND
                    || owner.currentActionNode.associatedJobType == JOB_TYPE.REPORT_CORRUPTED_STRUCTURE
                    || owner.currentActionNode.associatedJobType == JOB_TYPE.RESTRAIN) {
                    SetIsRunning(true);
                    return;
                }
            } else if (owner.stateComponent.currentState != null) {
                if (owner.stateComponent.currentState.characterState == CHARACTER_STATE.DOUSE_FIRE) {
                    SetIsRunning(true);
                    return;
                }
            }
        }
    }
    public void AdjustUseRunSpeed(int amount) {
        useRunSpeed += amount;
        useRunSpeed = Mathf.Max(0, useRunSpeed);
    }
    public bool CanStillPursueTarget(Character target) {
        if (isRunning) {
            return true;
        } else {
            return false;
        }
    }
    public void SetHasMovedOnCorruption(bool state) {
        hasMovedOnCorruption = state;
    }
    public void SetIsStationary(bool state) {
        isStationary = state;
    } 

    #region Go To
    public bool GoToLocation(Region targetLocation, PATHFINDING_MODE pathfindingMode, LocationStructure targetStructure = null,
        Action doneAction = null, Action actionOnStartOfMovement = null, IPointOfInterest targetPOI = null, LocationGridTile targetTile = null) {
        if (owner.avatar.isTravelling && owner.avatar.travelLine != null) {
            return true;
        }
        if (owner.currentRegion == targetLocation) {
            //action doer is already at the target location
            doneAction?.Invoke();
            return true;
        } else {
            //_icon.SetActionOnTargetReached(doneAction);
            LocationGridTile exitTile = owner.GetTargetTileToGoToRegion(targetLocation.coreTile.region);
            if (owner.movementComponent.HasPathTo(exitTile)) {
                //check first if character has path toward the exit tile.
                owner.marker.GoTo(exitTile, () => MoveToAnotherLocation(targetLocation.coreTile.region, pathfindingMode, targetStructure, doneAction, actionOnStartOfMovement, targetPOI, targetTile));
                return true;
            } else {
                return false;
            }
        }
    }
    private void MoveToAnotherLocation(Region targetLocation, PATHFINDING_MODE pathfindingMode, LocationStructure targetStructure = null,
        Action doneAction = null, Action actionOnStartOfMovement = null, IPointOfInterest targetPOI = null, LocationGridTile targetTile = null) {
        owner.avatar.SetTarget(targetLocation, targetStructure, targetPOI, targetTile);
        owner.avatar.StartPath(PATHFINDING_MODE.PASSABLE, doneAction, actionOnStartOfMovement);
    }
    /// <summary>
    /// Move this character to another structure in the same npcSettlement.
    /// </summary>
    /// <param name="newStructure">New structure the character is going to.</param>
    /// <param name="destinationTile">LocationGridTile where the character will go to (Must be inside the new structure).</param>
    /// <param name="targetPOI">The Point of Interest this character will interact with</param>
    /// <param name="arrivalAction">What should this character do when it reaches its target tile?</param>
    public void MoveToAnotherStructure(LocationStructure newStructure, LocationGridTile destinationTile, IPointOfInterest targetPOI = null, Action arrivalAction = null) {
        if (isStationary) {
            return;
        }
        //if the character is already at the destination tile, just do the specified arrival action, if any.
        if (owner.gridTileLocation == destinationTile) {
            if (arrivalAction != null) {
                arrivalAction();
            }
            //marker.PlayIdle();
        } else {
            if (destinationTile == null) {
                if (targetPOI != null) {
                    //if destination tile is null, make the charater marker use target poi logic (Usually used for moving targets)
                    owner.marker.GoToPOI(targetPOI, arrivalAction);
                } else {
                    if (arrivalAction != null) {
                        arrivalAction();
                    }
                }
            } else {
                //if destination tile is not null, got there, regardless of target poi
                owner.marker.GoTo(destinationTile, arrivalAction);
            }

        }
    }
    #endregion

    #region Pathfinding
    public bool HasPathTo(LocationGridTile toTile) {
        LocationGridTile fromTile = owner.gridTileLocation;
        if (!CanDig()) {
            return PathfindingManager.Instance.HasPath(fromTile, toTile);
        } else {
            if (fromTile == null || toTile == null) { return false; }
            if (fromTile == toTile) { return true; }

            //If digging is enabled, always return true, because the digging will handle the blocked path
            return true;
        }
    }
    public bool HasPathTo(HexTile toTile) {
        LocationGridTile targetTile = CollectionUtilities.GetRandomElement(toTile.locationGridTiles);
        return HasPathTo(targetTile);
    }
    /// <summary>
    /// Does this character have a path towards the target tile?
    /// Even if that tile is part of a different region?
    /// </summary>
    /// <param name="toTile">The target tile</param>
    /// <param name="allowDiggingWhenChecking">Should this function take into account whether or not digging has been enabled for this character</param>
    /// <returns></returns>
    public bool HasPathToEvenIfDiffRegion(LocationGridTile toTile, bool allowDiggingWhenChecking = true) {
        LocationGridTile fromTile = owner.gridTileLocation;
        if (allowDiggingWhenChecking && CanDig()) {
            if (fromTile == null || toTile == null) { return false; }
            if (fromTile == toTile) { return true; }

            //If digging is enabled, always return true, because the digging will handle the blocked path
            return true;
        } else {
            return PathfindingManager.Instance.HasPathEvenDiffRegion(fromTile, toTile);
        }
    }
    #endregion

    #region Dig
    public bool CanDig() {
        //Must not dig out of Kennel
        //https://trello.com/c/Yyj9DFry/1582-some-monsters-can-dig-out-of-kennel
        if (enableDigging && owner.currentStructure.structureType != STRUCTURE_TYPE.KENNEL) {
            if(owner.combatComponent.isInCombat) {
                if(!(owner.stateComponent.currentState as CombatState).isAttacking || (owner.marker && owner.marker.hasFleePath)) {
                    return false;
                }
            }
            return true;
        }
        return false;
    }
    public void SetEnableDigging(bool state) {
        if (state) {
            _enableDiggingCounter++;
        } else {
            _enableDiggingCounter--;
        }
    }
    public bool DigOnReachEndPath(Path path) {
        Vector3 lastPositionInPath = path.vectorPath.Last();
        //no path to target tile
        //create job to dig wall
        LocationGridTile targetTile;

        LocationGridTile tile = owner.currentRegion.innerMap.GetTile(lastPositionInPath);
        if (tile.objHere is BlockWall) {
            targetTile = tile;
        } else {
            Vector2 direction = lastPositionInPath - tile.centeredWorldLocation; //character.behaviourComponent.currentAbductTarget.worldPosition - tile.centeredWorldLocation;
            if (direction.y > 0) {
                //north
                targetTile = tile.GetNeighbourAtDirection(GridNeighbourDirection.North);
            } else if (direction.y < 0) {
                //south
                targetTile = tile.GetNeighbourAtDirection(GridNeighbourDirection.South);
            } else if (direction.x > 0) {
                //east
                targetTile = tile.GetNeighbourAtDirection(GridNeighbourDirection.East);
            } else {
                //west
                targetTile = tile.GetNeighbourAtDirection(GridNeighbourDirection.West);
            }
            if (targetTile != null && targetTile.objHere == null) {
                for (int i = 0; i < targetTile.neighbourList.Count; i++) {
                    LocationGridTile neighbour = targetTile.neighbourList[i];
                    if (neighbour.objHere is BlockWall) {
                        targetTile = neighbour;
                        break;
                    }
                }
            }
        }


        //Debug.Log($"No Path found for {owner.name} towards {owner.behaviourComponent.currentAbductTarget?.name ?? "null"}! Last position in path is {lastPositionInPath.ToString()}. Wall to dig is at {targetTile}");
        //Assert.IsNotNull(targetTile.objHere, $"Object at {targetTile} is null, but {owner.name} wants to dig it.");

        if(targetTile != null && targetTile.objHere != null && targetTile.objHere is BlockWall) {
            if (!owner.jobQueue.HasJob(JOB_TYPE.DIG_THROUGH)) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DIG_THROUGH, INTERACTION_TYPE.DIG, targetTile.objHere, owner);
                job.SetCannotBePushedBack(true);
                owner.jobQueue.AddJobInQueue(job);
                return true;
            }
        }
        return false;
        // character.behaviourComponent.SetDigForAbductionPath(null); //so behaviour can be run again after job has been added
    }
    #endregion
}
