using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Pathfinding;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;
using UtilityScripts;

public class MovementComponent : CharacterComponent {
    public bool isRunning { get; private set; }
    public bool noRunExceptCombat { get; private set; }
    public bool noRunWithoutException { get; private set; }
    public int useRunSpeed { get; private set; }
    public float speedModifier { get; private set; }
    public float walkSpeedModifier { get; private set; }
    public float runSpeedModifier { get; private set; }
    public bool hasMovedOnCorruption { get; private set; }
    public bool isStationary { get; private set; }
    public bool cameFromWurmHole { get; private set; }
    public bool isTravellingInWorld { get; private set; }
    public Region targetRegionToTravelInWorld { get; private set; }
    public List<LocationStructure> structuresToAvoid { get; }
    public int enableDiggingCounter { get; private set; }
    public int avoidSettlementsCounter { get; private set; }
    public int traversableTags { get; private set; } 
    public int[] tagPenalties { get; private set; }

    #region getters
    public float walkSpeed => owner.raceSetting.walkSpeed + (owner.raceSetting.walkSpeed * walkSpeedModifier);
    public float runSpeed => owner.raceSetting.runSpeed + (owner.raceSetting.runSpeed * runSpeedModifier);
    public bool enableDigging => enableDiggingCounter > 0;
    public bool avoidSettlements => avoidSettlementsCounter > 0;
    #endregion

    public MovementComponent() {
        structuresToAvoid = new List<LocationStructure>();
        tagPenalties = new int[32];
        traversableTags = InnerMapManager.All_Tags; //enable all tags for now 
        SetTagAsUnTraversable(InnerMapManager.Obstacle_Tag); //by default all units cannot traverse obstacle tag
    }
    public MovementComponent(SaveDataMovementComponent data) {
        structuresToAvoid = new List<LocationStructure>();

        isRunning = data.isRunning;
        noRunExceptCombat = data.noRunExceptCombat;
        noRunWithoutException = data.noRunWithoutException;
        useRunSpeed = data.useRunSpeed;
        speedModifier = data.speedModifier;
        walkSpeedModifier = data.walkSpeedModifier;
        runSpeedModifier = data.runSpeedModifier;
        hasMovedOnCorruption = data.hasMovedOnCorruption;
        isStationary = data.isStationary;
        cameFromWurmHole = data.cameFromWurmHole;
        isTravellingInWorld = data.isTravellingInWorld;
        enableDiggingCounter = data.enableDiggingCounter;
        avoidSettlementsCounter = data.avoidSettlementsCounter;
        traversableTags = data.traversableTags;
        tagPenalties = data.tagPenalties;
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
    public void SetAvoidSettlements(bool state) {
        if (state) {
            avoidSettlementsCounter++;
        } else {
            avoidSettlementsCounter--;
        }
    }
    public void SetCameFromWurmHole(bool state) {
        cameFromWurmHole = state;
    }

    #region Go To
    public bool MoveToAnotherRegion(Region targetRegion, Action doneAction = null) {
        if (owner.currentRegion == targetRegion) {
            //action doer is already at the target location
            doneAction?.Invoke();
            return true;
        } else {
            LocationGridTile gate = owner.GetTargetTileToGoToRegion(targetRegion);
            DIRECTION direction = gate.GetDirection();
            LocationGridTile exitTile = owner.gridTileLocation.GetNearestEdgeTileFromThis(direction);
            if (exitTile != null && owner.movementComponent.HasPathTo(exitTile)) {
                //check first if character has path toward the exit tile.
                owner.marker.GoTo(exitTile, () => TravelToAnotherRegion(targetRegion, doneAction));
                return true;
            }
        }
        return false;
    }
    private void TravelToAnotherRegion(Region targetRegion, Action doneAction = null) {
        if(!owner.canPerform || !owner.canMove || owner.isDead) {
            return;
        }
        StartTravellingToRegion(targetRegion, doneAction);
    }
    private void StartTravellingToRegion(Region targetRegion, Action doneAction = null) {
        if (isTravellingInWorld) {
            owner.logComponent.PrintLogErrorIfActive(owner.name + " cannot travel to " + targetRegion.name + " because it is already travelling in the world");
            return;
        }
        isTravellingInWorld = true;
        SetTargetRegionToTravelInWorld(targetRegion);
        owner.SetPOIState(POI_STATE.INACTIVE);
        if (owner.carryComponent.isCarryingAnyPOI) {
            owner.carryComponent.carriedPOI.SetPOIState(POI_STATE.INACTIVE);
        }

        Log leftLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Generic", "left_location", providedTags: LOG_TAG.Life_Changes);
        leftLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER, false);
        leftLog.AddToFillers(owner.currentRegion, owner.currentRegion.name, LOG_IDENTIFIER.LANDMARK_1);
        leftLog.AddLogToDatabase();
        owner.DisableMarker();

        owner.combatComponent.ClearHostilesInRange();
        owner.combatComponent.ClearAvoidInRange();

        if (owner.marker) {
            owner.marker.ClearPOIsInVisionRange();
            owner.marker.ClearPOIsInVisionRangeButDiffStructure();
            owner.marker.pathfindingAI.ClearAllCurrentPathData();
        }

        Messenger.Broadcast(Signals.STARTED_TRAVELLING_IN_WORLD, owner);

        FinishTravellingToRegion(doneAction);
    }
    private void FinishTravellingToRegion(Action doneAction = null) {
        if (!isTravellingInWorld) {
            owner.logComponent.PrintLogErrorIfActive(owner.name + " cannot finish travel to " + targetRegionToTravelInWorld?.name + " because it is already not travelling in the world");
            return;
        }
        isTravellingInWorld = false;

        if (!owner.marker) {
            owner.CreateMarker();
        }

        Region fromRegion = owner.currentRegion;
        fromRegion.RemoveCharacterFromLocation(owner);

        //character must arrive at the direction that it came from.
        LocationGridTile entrance = (targetRegionToTravelInWorld.innerMap as RegionInnerTileMap).GetTileToGoToRegion(fromRegion);//targetLocation.innerMap.GetRandomUnoccupiedEdgeTile();
        owner.marker.PlaceMarkerAt(entrance);

        owner.SetPOIState(POI_STATE.ACTIVE);
        if (owner.carryComponent.isCarryingAnyPOI) {
            owner.carryComponent.carriedPOI.SetPOIState(POI_STATE.ACTIVE);
        }
        Log arriveLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Generic", "arrive_location", providedTags: LOG_TAG.Life_Changes);
        arriveLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER, false);
        arriveLog.AddToFillers(targetRegionToTravelInWorld, targetRegionToTravelInWorld.name, LOG_IDENTIFIER.LANDMARK_1);
        arriveLog.AddLogToDatabase();

        if (owner.isNormalCharacter) {
            PlayerManager.Instance.player.ShowNotificationFrom(targetRegionToTravelInWorld, arriveLog);
        }

        owner.EnableMarker();
        SetTargetRegionToTravelInWorld(null);

        Messenger.Broadcast(Signals.FINISHED_TRAVELLING_IN_WORLD, owner);

        if (doneAction != null) {
            doneAction();
        }
    }
    public void SetTargetRegionToTravelInWorld(Region region) {
        targetRegionToTravelInWorld = region;
    }
    #endregion

    #region Pathfinding
    /// <summary>
    /// Does this character have a path towards the target tile?
    /// Note: This factors in digging capabilities. If need to query without
    /// digging use <see cref="PathfindingManager.HasPath"/>
    /// </summary>
    /// <param name="toTile">The target tile</param>
    /// <returns>True or false.</returns>
    public bool HasPathTo(LocationGridTile toTile) {
        LocationGridTile fromTile = owner.gridTileLocation;
        if (!CanDig()) {
            if (owner.traitContainer.HasTrait("Vampire")) {
                //Always has path if the character is a vampire and there are no non hostile villager in range that considers vampire a crime because he can just switch to bat form and move through walls
                if (!owner.crimeComponent.HasNonHostileVillagerInRangeThatConsidersCrimeTypeACrime(CRIME_TYPE.Vampire)) {
                    if (fromTile == null || toTile == null) { return false; }
                    if (fromTile == toTile) { return true; }

                    return true;
                }
            }
            return PathfindingManager.Instance.HasPath(fromTile, toTile);
        } else {
            if (fromTile == null || toTile == null) { return false; }
            if (fromTile == toTile) { return true; }

            if(toTile.groundType != LocationGridTile.Ground_Type.Water) {
                //If digging is enabled, always return true, because the digging will handle the blocked path
                return true;
            }
            return false;
        }
    }
    public bool HasPathTo(HexTile toTile) {
        LocationGridTile targetTile = CollectionUtilities.GetRandomElement(toTile.locationGridTiles);
        return HasPathTo(targetTile);
    }
    /// <summary>
    /// Does this character have a path towards the target tile?
    /// Even if that tile is part of a different region?
    /// Note: This factors in digging capabilities. If need to query without
    /// digging use <see cref="PathfindingManager.HasPathEvenDiffRegion(LocationGridTile, LocationGridTile)"/>
    /// </summary>
    /// <param name="toTile">The target tile</param>
    /// <returns>True or false.</returns>
    public bool HasPathToEvenIfDiffRegion(LocationGridTile toTile) {
        LocationGridTile fromTile = owner.gridTileLocation;
        if (CanDig()) {
            if (fromTile == null || toTile == null) { return false; }
            if (fromTile == toTile) { return true; }

            //If digging is enabled, always return true, because the digging will handle the blocked path
            if (toTile.groundType != LocationGridTile.Ground_Type.Water) {
                return true;
            }
            return false;
        } else {
            if (owner.traitContainer.HasTrait("Vampire")) {
                //Always has path if the character is a vampire and there are no non hostile villager in range that considers vampire a crime because he can just switch to bat form and move through walls
                if (!owner.crimeComponent.HasNonHostileVillagerInRangeThatConsidersCrimeTypeACrime(CRIME_TYPE.Vampire)) {
                    if (fromTile == null || toTile == null) { return false; }
                    if (fromTile == toTile) { return true; }

                    return true;
                }
            }
            return PathfindingManager.Instance.HasPathEvenDiffRegion(fromTile, toTile);
        }
    }
    /// <summary>
    /// Does this character have a path towards the target tile?
    /// Even if that tile is part of a different region?
    /// Note: This factors in digging capabilities. If need to query without
    /// digging use <see cref="PathfindingManager.HasPathEvenDiffRegion(LocationGridTile, LocationGridTile, NNConstraint)"/>
    /// </summary>
    /// <param name="toTile">The target tile</param>
    /// <param name="constraint">The constraint to use when performing the query</param>
    /// <returns>True or false.</returns>
    public bool HasPathToEvenIfDiffRegion(LocationGridTile toTile, NNConstraint constraint) {
        LocationGridTile fromTile = owner.gridTileLocation;
        if (CanDig()) {
            if (fromTile == null || toTile == null) { return false; }
            if (fromTile == toTile) { return true; }

            //If digging is enabled, always return true, because the digging will handle the blocked path
            return true;
        } else {
            if (owner.traitContainer.HasTrait("Vampire")) {
                //Always has path if the character is a vampire and there are no non hostile villager in range that considers vampire a crime because he can just switch to bat form and move through walls
                if (!owner.crimeComponent.HasNonHostileVillagerInRangeThatConsidersCrimeTypeACrime(CRIME_TYPE.Vampire)) {
                    if (fromTile == null || toTile == null) { return false; }
                    if (fromTile == toTile) { return true; }

                    return true;
                }
            }
            return PathfindingManager.Instance.HasPathEvenDiffRegion(fromTile, toTile, constraint);
        }
    }
    #endregion

    #region Dig
    public bool CanDig() {
        //Must not dig out of Kennel
        //https://trello.com/c/Yyj9DFry/1582-some-monsters-can-dig-out-of-kennel
        if (enableDigging && owner.currentStructure != null && owner.currentStructure.structureType != STRUCTURE_TYPE.KENNEL) {
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
            enableDiggingCounter++;
        } else {
            enableDiggingCounter--;
        }
    }
    public LocationGridTile GetBlockerTargetTileOnReachEndPath(Path path, LocationGridTile lastGridTileInPath) {
        LocationGridTile targetTile = null;

        if (!owner.marker) {
            return null;
        }

        LocationGridTile tile = lastGridTileInPath;// owner.currentRegion.innerMap.GetTile(lastPositionInPath);
        if (tile.objHere is BlockWall || tile.centeredWorldLocation == owner.marker.transform.position) {
            targetTile = tile;
        } else {
            Vector2 direction = tile.centeredWorldLocation - owner.marker.transform.position; //character.behaviourComponent.currentAbductTarget.worldPosition - tile.centeredWorldLocation;
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
        }
        //We must not check the neighbours of neighbours
        if (targetTile != null && (targetTile.objHere == null || !(targetTile.objHere is BlockWall))) {
            LocationGridTile newTargetTile = null;
            for (int i = 0; i < tile.neighbourList.Count; i++) {
                LocationGridTile neighbour = tile.neighbourList[i];
                if (neighbour.objHere is BlockWall) {
                    newTargetTile = neighbour;
                    break;
                }
            }
            targetTile = newTargetTile;
        }
        return targetTile;
    }
    public bool DigOnReachEndPath(Path path, LocationGridTile lastGridTileInPath) {
        //Vector3 lastPositionInPath = path.vectorPath.Last();

        //no path to target tile
        //create job to dig wall
        LocationGridTile targetTile = GetBlockerTargetTileOnReachEndPath(path, lastGridTileInPath);


        //Debug.Log($"No Path found for {owner.name} towards {owner.behaviourComponent.currentAbductTarget?.name ?? "null"}! Last position in path is {lastPositionInPath.ToString()}. Wall to dig is at {targetTile}");
        //Assert.IsNotNull(targetTile.objHere, $"Object at {targetTile} is null, but {owner.name} wants to dig it.");

        if (targetTile != null && targetTile.objHere != null && targetTile.objHere is BlockWall) {
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
    public bool AttackBlockersOnReachEndPath(Path path, LocationGridTile lastGridTileInPath) {
        LocationGridTile targetTile = GetBlockerTargetTileOnReachEndPath(path, lastGridTileInPath);

        if (targetTile != null && targetTile.objHere != null && targetTile.objHere is BlockWall) {
            if (owner.combatComponent.hostilesInRange.Contains(targetTile.objHere)) {
                owner.combatComponent.SetWillProcessCombat(true);
            } else {
                owner.combatComponent.Fight(targetTile.objHere, CombatManager.Dig);
            }
            return true;
        }
        return false;
    }
    #endregion

    #region Avoid Structures
    private bool AddStructureToAvoid(LocationStructure locationStructure) {
        if (!structuresToAvoid.Contains(locationStructure)) {
            structuresToAvoid.Add(locationStructure);
            owner.logComponent.PrintLogIfActive($"{owner.name} has added {locationStructure} to its structure avoid list!");
            return true;
        }
        return false;
    }
    public void RemoveStructureToAvoid(LocationStructure locationStructure) {
        if (structuresToAvoid.Remove(locationStructure)) {
            owner.logComponent.PrintLogIfActive($"{owner.name} has removed {locationStructure} from its structure avoid list!");
        }
    }
    public void AddStructureToAvoidAndScheduleRemoval(LocationStructure locationStructure) {
        if (AddStructureToAvoid(locationStructure)) {
            GameDate expiryDate = GameManager.Instance.Today();
            expiryDate.AddDays(1);
            SchedulingManager.Instance.AddEntry(expiryDate, () => RemoveStructureToAvoid(locationStructure), this);
            if (owner.homeSettlement != null && GameUtilities.RollChance(25) && owner.faction != null && !owner.faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Extermination, locationStructure)) {
                if (locationStructure.settlementLocation == null || locationStructure.settlementLocation.HasAliveResidentThatIsHostileWith(owner.faction)) {
                    owner.faction.partyQuestBoard.CreateExterminatePartyQuest(owner, owner.homeSettlement, locationStructure, owner.homeSettlement);
                }
                //owner.homeSettlement.settlementJobTriggerComponent.TriggerExterminationJob(locationStructure);
            }
        }
    }
    #endregion

    #region Tags
    public void SetTagAsTraversable(int tag) {
        traversableTags = traversableTags | tag;
        if (owner != null && owner.marker != null) {
            owner.marker.UpdateTraversableTags();
        }
    }
    public void SetTagAsUnTraversable(int tag) {
        traversableTags = traversableTags & ~tag;
        if (owner != null && owner.marker != null) {
            owner.marker.UpdateTraversableTags();
        }
    }
    public void SetPenaltyForTag(int tag, int penalty) {
        tagPenalties[tag] = penalty;
        if (owner != null && owner.marker != null) {
            owner.marker.UpdateTagPenalties();
        }
    }
    #endregion
    
    #region Loading
    public void LoadReferences(SaveDataMovementComponent data) {
        if (!string.IsNullOrEmpty(data.targetRegionToTravelInWorld)) {
            targetRegionToTravelInWorld = DatabaseManager.Instance.regionDatabase.GetRegionByPersistentID(data.targetRegionToTravelInWorld);
        }

        for (int i = 0; i < data.structuresToAvoid.Count; i++) {
            LocationStructure structure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(data.structuresToAvoid[i]);
            structuresToAvoid.Add(structure);
        }

    }
    #endregion
}

[System.Serializable]
public class SaveDataMovementComponent : SaveData<MovementComponent> {
    public bool isRunning;
    public bool noRunExceptCombat;
    public bool noRunWithoutException;
    public int useRunSpeed;
    public float speedModifier;
    public float walkSpeedModifier;
    public float runSpeedModifier;
    public bool hasMovedOnCorruption;
    public bool isStationary;
    public bool cameFromWurmHole;
    public bool isTravellingInWorld;
    public string targetRegionToTravelInWorld;
    public List<string> structuresToAvoid;

    public int enableDiggingCounter;
    public int avoidSettlementsCounter;
    public int traversableTags;
    public int[] tagPenalties;

    #region Overrides
    public override void Save(MovementComponent data) {
        isRunning = data.isRunning;
        noRunExceptCombat = data.noRunExceptCombat;
        noRunWithoutException = data.noRunWithoutException;
        useRunSpeed = data.useRunSpeed;
        speedModifier = data.speedModifier;
        walkSpeedModifier = data.walkSpeedModifier;
        runSpeedModifier = data.runSpeedModifier;
        hasMovedOnCorruption = data.hasMovedOnCorruption;
        isStationary = data.isStationary;
        cameFromWurmHole = data.cameFromWurmHole;
        isTravellingInWorld = data.isTravellingInWorld;

        if(data.targetRegionToTravelInWorld != null) {
            targetRegionToTravelInWorld = data.targetRegionToTravelInWorld.persistentID;
        }

        structuresToAvoid = new List<string>();
        for (int i = 0; i < data.structuresToAvoid.Count; i++) {
            structuresToAvoid.Add(data.structuresToAvoid[i].persistentID);
        }

        enableDiggingCounter = data.enableDiggingCounter;
        avoidSettlementsCounter = data.avoidSettlementsCounter;
        traversableTags = data.traversableTags;
        tagPenalties = data.tagPenalties;
    }

    public override MovementComponent Load() {
        MovementComponent component = new MovementComponent(this);
        return component;
    }
    #endregion
}