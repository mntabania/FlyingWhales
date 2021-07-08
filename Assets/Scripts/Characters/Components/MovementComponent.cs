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
    public bool isFlying => owner.traitContainer.HasTrait("Flying");
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
        //SetEnableDigging(true);
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
        //isFlying = data.isFlying;
        enableDiggingCounter = data.enableDiggingCounter;
        avoidSettlementsCounter = data.avoidSettlementsCounter;
        traversableTags = data.traversableTags;
        tagPenalties = data.tagPenalties;
    }

    public void SubscribeToSignals() {
        Messenger.AddListener<Character>(CharacterSignals.STARTED_TRAVELLING, OnStartedTravelling);
        Messenger.AddListener<Faction>(FactionSignals.FACTION_DISBANDED, OnFactionDisbanded);
    }
    public void UnsubscribeFromSignals() {
        Messenger.RemoveListener<Character>(CharacterSignals.STARTED_TRAVELLING, OnStartedTravelling);
        Messenger.RemoveListener<Faction>(FactionSignals.FACTION_DISBANDED, OnFactionDisbanded);
    }

    public void UpdateSpeed() {
        if (owner.marker) {
            SetMovementState();
            owner.marker.pathfindingAI.speed = GetSpeed();
            Messenger.Broadcast(CharacterSignals.UPDATE_MOVEMENT_STATE, owner);
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
        bool isPartOfActiveParty = owner.partyComponent.isMemberThatJoinedQuest && owner.partyComponent.currentParty.isPlayerParty; //Party Walk Speed applies only on demon parties for now
        if (owner.partyComponent.hasParty && owner.partyComponent.currentParty.isActive && owner.partyComponent.currentParty.currentQuest is DemonDefendPartyQuest) {
            //Do not use party walk speed on demon defend quest
            isPartOfActiveParty = false;
        }
        if (!isRunning) {
            speed = walkSpeed;
        }
        speed += (speed * speedModifier);
        if (speed <= 0f) {
            speed = 0.5f;
        }
        if (isPartOfActiveParty && !isRunning) {
            speed = Mathf.Min(owner.partyComponent.currentParty.partyWalkSpeed, speed);
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
                    || owner.currentActionNode.associatedJobType == JOB_TYPE.RESTRAIN
                    || (owner.currentActionNode.associatedJobType == JOB_TYPE.CAPTURE_CHARACTER && owner.race == RACE.HARPY)
                    || owner.currentActionNode.associatedJobType == JOB_TYPE.TRITON_KIDNAP) {
                    SetIsRunning(true);
                    return;
                }
            } else if (owner.stateComponent.currentState != null) {
                if (owner.stateComponent.currentState.characterState == CHARACTER_STATE.DOUSE_FIRE) {
                    SetIsRunning(true);
                    return;
                }
            } else if (owner.partyComponent.isFollowingBeacon) {
                SetIsRunning(true);
                return;
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
    public void SetToFlying() {
        owner.traitContainer.AddTrait(owner, "Flying");
        SetTagAsTraversable(InnerMapManager.Obstacle_Tag);
    }

    public void SetToNonFlying() {
        owner.traitContainer.RemoveTrait(owner, "Flying");
        SetTagAsUnTraversable(InnerMapManager.Obstacle_Tag);
    }

    #region Listeners
    private void OnStartedTravelling(Character character) {
        OnCharacterStartedTravelling(character);
    }
    public void OnAssignedClass(CharacterClass characterClass) {
        if (characterClass.className == "Ratman") {
            AvoidAllFactions();
        }
    }
    public void OnChangeFactionTo(Faction newFaction) {
        if (newFaction != null) {
            DoNotAvoidFaction(newFaction);
        }
    }
    private void OnFactionDisbanded(Faction p_faction) {
        DoNotAvoidFaction(p_faction); //clear out any avoidance of the faction, this is to ensure no conflicts will arise if a new faction will use the disbanded factions tags
    }
    #endregion

    #region Go To
    public bool MoveToAnotherRegion(Region targetRegion, Action doneAction = null) {
        if (owner.currentRegion == targetRegion) {
            //action doer is already at the target location
            doneAction?.Invoke();
            return true;
        } else {
            LocationGridTile exitTile = owner.gridTileLocation.GetExitTileToGoToRegion(targetRegion);
            if (exitTile != null && owner.movementComponent.HasPathTo(exitTile)) {
                //check first if character has path toward the exit tile.
                owner.marker.GoTo(exitTile, () => TravelToAnotherRegion(targetRegion, doneAction));
                return true;
            }
        }
        return false;
    }
    private void TravelToAnotherRegion(Region targetRegion, Action doneAction = null) {
        if(!owner.limiterComponent.canPerform || !owner.limiterComponent.canMove || owner.isDead) {
            return;
        }
        StartTravellingToRegion(targetRegion, doneAction);
    }
    private void StartTravellingToRegion(Region targetRegion, Action doneAction = null) {
        if (isTravellingInWorld) {
#if DEBUG_LOG
            owner.logComponent.PrintLogErrorIfActive(owner.name + " cannot travel to " + targetRegion.name + " because it is already travelling in the world");
#endif
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
        leftLog.AddLogToDatabase(true);
        owner.DisableMarker();

        owner.combatComponent.ClearHostilesInRange();
        owner.combatComponent.ClearAvoidInRange();

        if (owner.marker) {
            owner.marker.ClearPOIsInVisionRange();
            owner.marker.ClearPOIsInVisionRangeButDiffStructure();
            owner.marker.pathfindingAI.ClearAllCurrentPathData();
        }

        Messenger.Broadcast(CharacterSignals.STARTED_TRAVELLING_IN_WORLD, owner);

        FinishTravellingToRegion(doneAction);
    }
    private void FinishTravellingToRegion(Action doneAction = null) {
        if (!isTravellingInWorld) {
#if DEBUG_LOG
            owner.logComponent.PrintLogErrorIfActive(owner.name + " cannot finish travel to " + targetRegionToTravelInWorld?.name + " because it is already not travelling in the world");
#endif
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
        arriveLog.AddLogToDatabase(true);

        if (owner.isNormalCharacter) {
            PlayerManager.Instance.player.ShowNotificationFrom(entrance, arriveLog);
        }

        owner.EnableMarker();
        SetTargetRegionToTravelInWorld(null);

        Messenger.Broadcast(CharacterSignals.FINISHED_TRAVELLING_IN_WORLD, owner);

        doneAction?.Invoke();
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
        if (!CanDig() && !isFlying) {
            if (owner.traitContainer.HasTrait("Vampire")) {
                //Always has path if the character is a vampire and there are no non hostile villager in range that considers vampire a crime because he can just switch to bat form and move through walls
                //if (!owner.crimeComponent.HasNonHostileVillagerInRangeThatConsidersCrimeTypeACrime(CRIME_TYPE.Vampire)) {
                    if (fromTile == null || toTile == null) { return false; }
                    if (fromTile == toTile) { return true; }

                    return true;
                //}
            }
            return PathfindingManager.Instance.HasPath(fromTile, toTile);
        } else {
            if (fromTile == null || toTile == null) { return false; }
            if (fromTile == toTile) { return true; }

            //If digging is enabled, always return true, because the digging will handle the blocked path
            if (toTile.groundType == LocationGridTile.Ground_Type.Water && !isFlying) {
                return false;
            }
            return true;
        }
    }
    public bool HasPathTo(Area toArea) {
        LocationGridTile targetTile = CollectionUtilities.GetRandomElement(toArea.gridTileComponent.gridTiles);
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
        if (CanDig() || isFlying) {
            if (fromTile == null || toTile == null) { return false; }
            if (fromTile == toTile) { return true; }

            //If digging is enabled, always return true, because the digging will handle the blocked path
            if (toTile.groundType == LocationGridTile.Ground_Type.Water && !isFlying) {
                return false;
            }
            return true;
        } else {
            if (owner.traitContainer.HasTrait("Vampire")) {
                //Always has path if the character is a vampire and there are no non hostile villager in range that considers vampire a crime because he can just switch to bat form and move through walls
                //if (!owner.crimeComponent.HasNonHostileVillagerInRangeThatConsidersCrimeTypeACrime(CRIME_TYPE.Vampire)) {
                    if (fromTile == null || toTile == null) { return false; }
                    if (fromTile == toTile) { return true; }

                    return true;
                //}
            }
            return PathfindingManager.Instance.HasPathEvenDiffRegion(fromTile, toTile);
        }
    }
    public bool HasPathToEvenIfDiffRegion(LocationStructure locationStructure) {
        if (locationStructure.passableTiles.Count > 0) {
            LocationGridTile randomTile = CollectionUtilities.GetRandomElement(locationStructure.passableTiles);
            return HasPathToEvenIfDiffRegion(randomTile);
        } else if (locationStructure.tiles.Count > 0) {
            LocationGridTile randomTile = CollectionUtilities.GetRandomElement(locationStructure.tiles);
            return HasPathToEvenIfDiffRegion(randomTile);
        } else {
            return false;
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
        if (CanDig() || isFlying) {
            if (fromTile == null || toTile == null) { return false; }
            if (fromTile == toTile) { return true; }

            //If digging is enabled, always return true, because the digging will handle the blocked path
            if (toTile.groundType == LocationGridTile.Ground_Type.Water && !isFlying) {
                return false;
            }
            return true;
        } else {
            if (owner.traitContainer.HasTrait("Vampire")) {
                //Always has path if the character is a vampire and there are no non hostile villager in range that considers vampire a crime because he can just switch to bat form and move through walls
                //if (!owner.crimeComponent.HasNonHostileVillagerInRangeThatConsidersCrimeTypeACrime(CRIME_TYPE.Vampire)) {
                    if (fromTile == null || toTile == null) { return false; }
                    if (fromTile == toTile) { return true; }

                    return true;
                //}
            }
            return PathfindingManager.Instance.HasPathEvenDiffRegion(fromTile, toTile, constraint);
        }
    }
    public bool CanReturnHome() {
        if (owner.HasHome()) {
            LocationGridTile chosenTile = null;
            if(owner.homeSettlement != null) {
                chosenTile = owner.homeSettlement.GetRandomPassableTile();
            } else if (owner.homeStructure != null) {
                chosenTile = owner.homeStructure.GetRandomPassableTile();
            } else if (owner.HasTerritory()) {
                chosenTile = owner.territory.GetRandomPassableTile();
            }
            if(chosenTile != null) {
                return HasPathToEvenIfDiffRegion(chosenTile);
            }
        }
        return false;
    }
#endregion

#region Dig
    public bool CanDig() {
        if (owner.currentJob != null) {
            if (owner.currentJob.jobType == JOB_TYPE.RESCUE_MOVE_CHARACTER) {
                //Rescuing paralyzed characters should be able to dig
                return true;
            } else if (owner.currentJob.jobType == JOB_TYPE.RESTRAIN && owner.currentJob.originalOwner is NPCSettlement) {
                //Allowed characters restraining for village to dig
                //Reference: https://trello.com/c/hT6r95jb/4845-villagers-restraining-harpy-inside-cave-issue
                return true;
            }
        }
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
    public LocationGridTile GetBlockerTargetTileOnReachEndPath(Path path, LocationGridTile lastGridTileInPath, LocationGridTile actualDestinationTile) {
        LocationGridTile targetTile = null;

        if (!owner.marker) {
            return null;
        }

        LocationGridTile tile = lastGridTileInPath;// owner.currentRegion.innerMap.GetTile(lastPositionInPath);
        if (tile.tileObjectComponent.HasWalls() || actualDestinationTile.centeredWorldLocation == tile.centeredWorldLocation) {
            targetTile = tile;
        } else {
            Vector2 direction = actualDestinationTile.centeredWorldLocation - tile.centeredWorldLocation; //character.behaviourComponent.currentAbductTarget.worldPosition - tile.centeredWorldLocation;
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
        if (targetTile != null && !targetTile.tileObjectComponent.HasWalls()) {
            LocationGridTile newTargetTile = null;
            for (int i = 0; i < tile.neighbourList.Count; i++) {
                LocationGridTile neighbour = tile.neighbourList[i];
                if (neighbour.tileObjectComponent.HasWalls()) {
                    newTargetTile = neighbour;
                    break;
                }
            }
            targetTile = newTargetTile;
        }
        return targetTile;
    }
    public bool DigOnReachEndPath(Path path, LocationGridTile lastGridTileInPath, LocationGridTile actualDestinationTile) {
        //Vector3 lastPositionInPath = path.vectorPath.Last();

        //no path to target tile
        //create job to dig wall
        LocationGridTile targetTile = GetBlockerTargetTileOnReachEndPath(path, lastGridTileInPath, actualDestinationTile);


        //Debug.Log($"No Path found for {owner.name} towards {owner.behaviourComponent.currentAbductTarget?.name ?? "null"}! Last position in path is {lastPositionInPath.ToString()}. Wall to dig is at {targetTile}");
        //Assert.IsNotNull(targetTile.tileObjectComponent.objHere, $"Object at {targetTile} is null, but {owner.name} wants to dig it.");

        if (targetTile != null && targetTile.tileObjectComponent.HasWalls()) {
            if (!owner.jobQueue.HasJob(JOB_TYPE.DIG_THROUGH)) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DIG_THROUGH, INTERACTION_TYPE.DIG, targetTile.tileObjectComponent.GetFirstWall(), owner);
                job.SetCannotBePushedBack(true);
                owner.jobQueue.AddJobInQueue(job);
                return true;
            }
        }
        return false;
        // character.behaviourComponent.SetDigForAbductionPath(null); //so behaviour can be run again after job has been added
    }
    public bool AttackBlockersOnReachEndPath(Path path, LocationGridTile lastGridTileInPath, LocationGridTile actualDestinationTile) {
        LocationGridTile targetTile = GetBlockerTargetTileOnReachEndPath(path, lastGridTileInPath, actualDestinationTile);

        if (targetTile != null && targetTile.tileObjectComponent.HasWalls()) {
            TileObject wall = targetTile.tileObjectComponent.GetFirstWall();
            if (owner.combatComponent.IsHostileInRange(wall)) {
                owner.combatComponent.SetWillProcessCombat(true);
            } else {
                owner.combatComponent.Fight(wall, CombatManager.Dig);
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
#if DEBUG_LOG
            owner.logComponent.PrintLogIfActive($"{owner.name} has added {locationStructure} to its structure avoid list!");
#endif
            return true;
        }
        return false;
    }
    public void RemoveStructureToAvoid(LocationStructure locationStructure) {
        if (structuresToAvoid.Remove(locationStructure)) {
#if DEBUG_LOG
            owner.logComponent.PrintLogIfActive($"{owner.name} has removed {locationStructure} from its structure avoid list!");
#endif
        }
    }
    public void AddStructureToAvoidAndScheduleRemoval(LocationStructure locationStructure) {
        if (AddStructureToAvoid(locationStructure)) {
            GameDate expiryDate = GameManager.Instance.Today();
            expiryDate.AddDays(1);
            SchedulingManager.Instance.AddEntry(expiryDate, () => RemoveStructureToAvoid(locationStructure), this);
            //if (owner.homeSettlement != null && owner.faction != null && !owner.faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Extermination, locationStructure)) {
            //    if (locationStructure.settlementLocation == null || locationStructure.settlementLocation.HasResidentThatIsNotDeadThatIsHostileWithFaction(owner.faction, owner)) {
            //        owner.faction.partyQuestBoard.CreateExterminatePartyQuest(owner, owner.homeSettlement, locationStructure, owner.homeSettlement);
            //    }
            //}
        }
    }
#endregion

#region Tags
    public void SetTagAsTraversable(int tag) {
        traversableTags = traversableTags | tag;
        if (owner != null && owner.hasMarker) {
            owner.marker.UpdateTraversableTags();
        }
    }
    public void SetTagAsUnTraversable(int tag) {
        traversableTags = traversableTags & ~tag;
        if (owner != null && owner.hasMarker) {
            owner.marker.UpdateTraversableTags();
        }
    }
    public void SetPenaltyForTag(int tag, int penalty) {
        tag -= 1; //had to subtract 1 since faction tags do not start at 0, but instead start at 1, causing a 1 index difference if they are in an array
        if (tag > 0) {
            tagPenalties[tag] = penalty;
            if (owner != null && owner.hasMarker) {
                owner.marker.UpdateTagPenalties();
            }    
        }
    }
    private void AvoidAllFactions() {
        for (int i = InnerMapManager.Starting_Tag_Index - 1; i < 32; i++) {
            SetPenaltyForTag(i, 100); //500
        }
    }
    private void DoNotAvoidFaction(Faction p_faction) {
        if (DoesFactionUsePathfindingTag(p_faction)) {
            int pathfindingTag = (int)p_faction.pathfindingTag;
            SetPenaltyForTag(pathfindingTag, 0);
            SetPenaltyForTag((int)p_faction.pathfindingDoorTag, 0);    
        }
    }
    private void AvoidFaction(Faction p_faction) {
        if (DoesFactionUsePathfindingTag(p_faction)) {
            int pathfindingTag = (int)p_faction.pathfindingTag;
            SetPenaltyForTag(pathfindingTag, 500); //500
            SetPenaltyForTag((int)p_faction.pathfindingDoorTag, 500); //500    
        }
    }
    public void RedetermineFactionsToAvoid(Character p_character) {
        if (p_character.faction != null) {
            foreach (var relationship in p_character.faction.relationships) {
                if (relationship.Value.relationshipStatus == FACTION_RELATIONSHIP_STATUS.Hostile) {
                    AvoidFaction(relationship.Key);
                } else {
                    DoNotAvoidFaction(relationship.Key);
                }
            }
        }
    }
    private bool DoesFactionUsePathfindingTag(Faction p_faction) {
        if (!p_faction.isMajorFaction) {
            return p_faction.factionType.type == FACTION_TYPE.Ratmen || p_faction.factionType.type == FACTION_TYPE.Undead; //Only undead and ratmen factions use pathfinding tags
        }
        return true; //All Major factions use pathfinding tags.
    }
#endregion

#region Travelling Status
    private void OnCharacterStartedTravelling(Character character) {
        if(owner != character) {
            if(owner.currentActionNode != null && owner.currentActionNode.poiTarget == character) {
                if(owner.currentActionNode.actionStatus == ACTION_STATUS.STARTED) {
                    if(owner.currentActionNode.associatedJobType == JOB_TYPE.RITUAL_KILLING
                        || owner.currentActionNode.goapType == INTERACTION_TYPE.SHARE_INFORMATION
                        || owner.currentActionNode.goapType == INTERACTION_TYPE.REPORT_CRIME) {
                        owner.currentActionNode.associatedJob?.ForceCancelJob();
                    }
                }
            }
        }
    }
#endregion

#region Combat Repositioning
    public bool IsCurrentGridNodeOccupiedByOtherNonRepositioningActiveCharacter() {
        LocationGridTile currentGridTile = owner.gridTileLocation;
        if (currentGridTile != null) {
            return currentGridTile.IsGridNodeOccupiedByNonRepositioningActiveCharacterOtherThan(owner);
        }
        return false;
    }
#endregion

#region Let Go
    public void LetGo(bool becomeDazed = false) {
        LocationStructure letGoFrom = owner.currentStructure;
        //Make character dazed (if not summon) and teleport him/her on a random spot outside
        List<LocationGridTile> allTilesOutside = RuinarchListPool<LocationGridTile>.Claim();
        List<LocationGridTile> passableTilesOutside = RuinarchListPool<LocationGridTile>.Claim();
        for (int i = 0; i < letGoFrom.tiles.Count; i++) {
            LocationGridTile tileInStructure = letGoFrom.tiles.ElementAt(i);
            for (int j = 0; j < tileInStructure.neighbourList.Count; j++) {
                LocationGridTile neighbour = tileInStructure.neighbourList[j];
                if (neighbour.structure is Wilderness && !allTilesOutside.Contains(neighbour)) {
                    allTilesOutside.Add(neighbour);
                    if (neighbour.IsPassable()) {
                        passableTilesOutside.Add(neighbour);
                    }
                }
            }
        }
        Assert.IsTrue(allTilesOutside.Count > 0);
        var targetTile = CollectionUtilities.GetRandomElement(passableTilesOutside.Count > 0 ? passableTilesOutside : allTilesOutside);
        if (becomeDazed) {
            if (owner is Summon == false) {
                owner.traitContainer.AddTrait(owner, "Dazed");
            }
        }
        CharacterManager.Instance.Teleport(owner, targetTile);
        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Minion_Dissipate);
        owner.traitContainer.RemoveRestrainAndImprison(owner);
        if (owner.isLycanthrope) {
            owner.lycanData.limboForm.traitContainer.RemoveRestrainAndImprison(owner.lycanData.limboForm);
        }
        RuinarchListPool<LocationGridTile>.Release(allTilesOutside);
        RuinarchListPool<LocationGridTile>.Release(passableTilesOutside);
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
    //public bool isFlying;
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
        //isFlying = data.isFlying;

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