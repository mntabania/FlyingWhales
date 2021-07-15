using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Goap.Unique_Action_Data;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Logs;
using Object_Pools;
using UnityEngine;
using Traits;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UtilityScripts;

//actual nodes located in a finished plan that is going to be executed by a character
public class ActualGoapNode : IRumorable, ICrimeable, ISavable, IObjectPoolTester {
    public string persistentID { get; private set; }
    public Character actor { get; private set; }
    public IPointOfInterest poiTarget { get; private set; }
    public Character disguisedActor { get; private set; }
    public Character disguisedTarget { get; private set; }
    public bool isStealth { get; private set; }
    public bool avoidCombat { get; private set; }
    public OtherData[] otherData { get; private set; }
    public int cost { get; private set; }
    public bool isIntel { get; private set; }
    public bool isNegativeInfo { get; private set; }
    public bool hasBeenReset { get; private set; }
    public bool isSupposedToBeInPool { get; private set; }
    public bool hasStartedPerTickEffect { get; private set; }
    public int stillProcessingCounter { get; private set; }
    public bool isAssigned { get; set; }

    public int assignmentCounter { get; private set; }

    public GoapAction action { get; private set; }
    public ACTION_STATUS actionStatus { get; private set; }
    public Log thoughtBubbleLog { get; private set; } //used if the current state of this action has a duration
    public Log thoughtBubbleMovingLog { get; private set; } //used when the actor is moving with this as his/her current action
    public Log descriptionLog { get; private set; } //action log at the end of the action
    public LocationStructure targetStructure { get; private set; }
    public LocationGridTile targetTile { get; private set; }
    public IPointOfInterest targetPOIToGoTo { get; private set; }
    public JOB_TYPE associatedJobType { get; private set; }
    public JobQueueItem associatedJob { get; private set; }

    public string currentStateName { get; private set; }
    public int currentStateDuration { get; private set; }
    public Rumor rumor { get; private set; }
    public Assumption assumption { get; private set; }

    public List<Character> awareCharacters { get; private set; }
    public List<LOG_TAG> logTags { get; private set; }
    public int reactionProcessCounter { get; private set; } //Do not save this because the CharacterMarker does not save the 

    //Crime
    public CRIME_TYPE crimeType { get; private set; }
    public UniqueActionData uniqueActionData { get; private set; }

    #region getters
    public GoapActionState currentState {
        get {
            if (!action.states.ContainsKey(currentStateName)) {
                throw new System.Exception($"{action.goapName} does not have a state named {currentStateName}");
            }
            return action.states[currentStateName];
        }
    }
    public bool isPerformingActualAction => actionStatus == ACTION_STATUS.PERFORMING;
    public bool isDone => actionStatus == ACTION_STATUS.SUCCESS || actionStatus == ACTION_STATUS.FAIL;
    public INTERACTION_TYPE goapType => action.goapType;
    public string goapName => action.goapName;
    public bool isRumor => rumor != null;
    public bool isAssumption => assumption != null;
    public string name => action.goapName;
    public string classificationName => "News";
    public IPointOfInterest target => poiTarget;
    public Log informationLog => descriptionLog;
    public RUMOR_TYPE rumorType => RUMOR_TYPE.Action;
    public CRIMABLE_TYPE crimableType => CRIMABLE_TYPE.Action;
    public OBJECT_TYPE objectType => OBJECT_TYPE.Action;
    public System.Type serializedData => typeof(SaveDataActualGoapNode);
    public bool isStillProcessing => stillProcessingCounter > 0;
    //public LOG_TAG[] logTags => GetLogTags().ToArray();
    #endregion

    public ActualGoapNode() {
        logTags = new List<LOG_TAG>();
        awareCharacters = new List<Character>();
    }

    public ActualGoapNode(SaveDataActualGoapNode data) {
        logTags = data.logTags;
        if (logTags == null) {
            logTags = new List<LOG_TAG>();
        }
        awareCharacters = new List<Character>();
        persistentID = data.persistentID;
        isStealth = data.isStealth;
        avoidCombat = data.avoidCombat;
        cost = data.cost;
        action = InteractionManager.Instance.goapActionData[data.action];
        actionStatus = data.actionStatus;
        associatedJobType = data.associatedJobType;
        currentStateName = data.currentStateName;
        currentStateDuration = data.currentStateDuration;
        crimeType = data.crimeType;
        isIntel = data.isIntel;
        isNegativeInfo = data.isNegativeInfo;
        isSupposedToBeInPool = data.isSupposedToBeInPool;
        stillProcessingCounter = data.stillProcessingCounter;
        hasBeenReset = data.hasBeenReset;
        isAssigned = data.isAssigned;
        hasStartedPerTickEffect = data.hasStartedPerTickEffect;
    }

    public void SetActionData(GoapAction action, Character actor, IPointOfInterest poiTarget, OtherData[] otherData, int cost) {
        persistentID = UtilityScripts.Utilities.GetNewUniqueID();
        this.action = action;
        this.actor = actor;
        this.poiTarget = poiTarget;
        this.otherData = otherData;
        this.cost = cost;
        //awareCharacters = new List<Character>();
        uniqueActionData = CreateUniqueActionData(action); //Object pool this
        //Whenever a disguised character is being set as actor/target, assign disguised actor/target
        disguisedActor = actor.reactionComponent.disguisedCharacter;
        if (poiTarget is Character targetCharacter) {
            disguisedTarget = targetCharacter.reactionComponent.disguisedCharacter;
        }
        SetDefaultLogTags();
        SetAdditionalLogTags();
        SetHasBeenReset(false);
        //Messenger.AddListener<string, ActualGoapNode>(Signals.ACTION_STATE_SET, OnActionStateSet);
    }


    //public void DestroyNode() {
    //    Messenger.RemoveListener<string, ActualGoapNode>(Signals.ACTION_STATE_SET, OnActionStateSet);
    //}

    #region Action
    public virtual void DoAction(JobQueueItem job, GoapPlan plan) {
        actionStatus = ACTION_STATUS.STARTED;
        associatedJobType = job.jobType; //We create a separate field for the job type that this action is connected instead of getting the job type from the associtatedJob because since the JobQueueItem is object pooled, there will be a time that the job will be brought back to the object pool when that happens the jobType will go back to NONE if we do not store it separately
        SetJob(job);
        isStealth = IsActionStealth(job);
        avoidCombat = IsActionAvoidCombat(job);
#if DEBUG_PROFILER
        Profiler.BeginSample($"Do Action - {actor.name} - {action.name} - Set Current Action Node");
#endif
        actor.SetCurrentActionNode(this, job, plan);
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
        // CreateThoughtBubbleLog(targetStructure);
        //parentPlan?.SetPlanState(GOAP_PLAN_STATE.IN_PROGRESS);
#if DEBUG_PROFILER
        Profiler.BeginSample($"Do Action - {actor.name} - {action.name} - Doing Action Signal");
#endif
        Messenger.Broadcast(JobSignals.CHARACTER_DOING_ACTION, actor, this);
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
        //actor.marker.UpdateActionIcon();
#if DEBUG_PROFILER
        Profiler.BeginSample($"Do Action - {actor.name} - {action.name} - On Action Started");
#endif
        action.OnActionStarted(this);
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
        //poiTarget.AddTargettedByAction(this);

        //Set Crime Type
#if DEBUG_PROFILER
        Profiler.BeginSample($"Do Action - {actor.name} - {action.name} - Set Crime Type");
#endif
        SetCrimeType();
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif

        //Move To Do Action
#if DEBUG_PROFILER
        Profiler.BeginSample($"Do Action - {actor.name} - {action.name} - Reset End Reached");
#endif
        actor.marker.pathfindingAI.ResetEndReachedDistance();
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif

#if DEBUG_PROFILER
        Profiler.BeginSample($"Do Action - {actor.name} - {action.name} - Set target to go to");
#endif
        SetTargetToGoTo();
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif

#if DEBUG_PROFILER
        Profiler.BeginSample($"Do Action - {actor.name} - {action.name} - Create Thought Bubble Log");
#endif
        CreateThoughtBubbleLog();
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif

#if DEBUG_PROFILER
        Profiler.BeginSample($"Do Action - {actor.name} - {action.name} - Check and move to do action");
#endif
        CheckAndMoveToDoAction(job);
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
    private void SetTargetToGoTo() {
        if (targetStructure == null) {
            targetStructure = action.GetTargetStructure(this);
            if (targetStructure == null) {
                //If target structure is null, instead of throwing an exception we just need to return the target tile as null
                //This would trigger the job to be cancelled because there is no target tile set
                //We are doing this to avoid game crashes, because when we throw an exception the game will be unplayable
                //But since we still do not allow actions with no target structure to continue, we need the job to be cancelled, hence, why we set the target tile to null
                targetTile = null;
                return;
            }
        }
        if (action.actionLocationType == ACTION_LOCATION_TYPE.NEAR_TARGET || action.actionLocationType == ACTION_LOCATION_TYPE.NEAR_OTHER_TARGET) {
            IPointOfInterest targetToGoTo = action.GetTargetToGoTo(this);
            if (targetToGoTo == null) {
                targetTile = action.GetTargetTileToGoTo(this);
            } else {
                targetPOIToGoTo = targetToGoTo;
                targetTile = targetToGoTo.gridTileLocation;
            }
            if (actor.movementComponent.isStationary) {
                if (actor.gridTileLocation != targetTile && !actor.gridTileLocation.IsNeighbour(targetTile, true)) {
                    targetTile = null;
                }
            }
        } else if (action.actionLocationType == ACTION_LOCATION_TYPE.IN_PLACE) {
            targetTile = actor.gridTileLocation;
        } else if (action.actionLocationType == ACTION_LOCATION_TYPE.NEARBY) {
            if (actor.limiterComponent.canMove && !actor.movementComponent.isStationary) {
                List<LocationGridTile> choices = ObjectPoolManager.Instance.CreateNewGridTileList();
                action.PopulateNearbyLocation(choices, this);
                if (choices.Count <= 0) {
                    actor.gridTileLocation.PopulateTilesInRadius(choices, 3, includeImpassable: false);
                }
                if (choices.Count > 0) {
                    targetTile = choices[UtilityScripts.Utilities.Rng.Next(0, choices.Count)];
                } else {
                    targetTile = actor.gridTileLocation;
                }
                ObjectPoolManager.Instance.ReturnGridTileListToPool(choices);
            } else {
                targetTile = actor.gridTileLocation;
            }
        } else if (action.actionLocationType == ACTION_LOCATION_TYPE.RANDOM_LOCATION) {
            if (actor.movementComponent.isStationary) {
                targetTile = null;
                return;
            }
            List<LocationGridTile> choices = targetStructure.passableTiles; //targetStructure.unoccupiedTiles.ToList();
            if (choices.Count > 0) {
                targetTile = choices[UtilityScripts.Utilities.Rng.Next(0, choices.Count)];
            }
            //else {
            //    throw new System.Exception(
            //        $"{actor.name} target tile of action {action.goapName} for {action.actionLocationType} is null.");
            //}
        } else if (action.actionLocationType == ACTION_LOCATION_TYPE.RANDOM_LOCATION_B) {
            if (actor.movementComponent.isStationary) {
                targetTile = null;
                return;
            }
            targetTile = action.GetTargetTileToGoTo(this);
            if (targetTile == null) {
                List<LocationGridTile> choices = RuinarchListPool<LocationGridTile>.Claim();
                for (int i = 0; i < targetStructure.unoccupiedTiles.Count; i++) {
                    LocationGridTile tile = targetStructure.unoccupiedTiles[i];
                    if (tile.HasUnoccupiedNeighbour(true)) {
                        choices.Add(tile);
                    }
                }
                //targetStructure.unoccupiedTiles.Where(x => x.UnoccupiedNeighbours.Count > 0).ToList();
                if (choices.Count > 0) {
                    targetTile = choices[UtilityScripts.Utilities.Rng.Next(0, choices.Count)];
                } else if (targetStructure.unoccupiedTiles.Count > 0) {
                    targetTile = targetStructure.unoccupiedTiles[UtilityScripts.Utilities.Rng.Next(0, targetStructure.unoccupiedTiles.Count)];
                } else if (targetStructure.tiles.Count > 0) {
                    //if all else fails return a random tile inside the target structure
                    targetTile = CollectionUtilities.GetRandomElement(targetStructure.tiles);
                } else if (actor.gridTileLocation != null) {
                    //if even the structure has no tiles, then just return the actors current location
                    targetTile = actor.gridTileLocation;
                }
                RuinarchListPool<LocationGridTile>.Release(choices);
                //else {
                //     throw new System.Exception(
                //    $"{actor.name} target tile of action {action.goapName} for {action.actionLocationType} is null.");  
                //}
            }
        } else if (action.actionLocationType == ACTION_LOCATION_TYPE.TARGET_IN_VISION) {
            if (actor.marker.IsPOIInVision(poiTarget)) {
                targetTile = actor.gridTileLocation;
            } else {
                if (actor.movementComponent.isStationary) {
                    targetTile = null;
                    return;
                }
                //No OnArriveAtTargetLocation because it doesn't trigger on arrival, rather, it is triggered by on vision
                IPointOfInterest targetToGoTo = action.GetTargetToGoTo(this);
                if (targetToGoTo == null) {
                    targetTile = action.GetTargetTileToGoTo(this);
                } else {
                    targetPOIToGoTo = targetToGoTo;
                    targetTile = targetToGoTo.gridTileLocation;
                }
            }
        } else if (action.actionLocationType == ACTION_LOCATION_TYPE.OVERRIDE) {
            LocationGridTile tile = action.GetOverrideTargetTile(this);
            if (tile != null) {
                targetTile = tile;
            }
            //else {
            //    throw new System.Exception(
            //        $"{actor.name} override target tile of action {action.goapName} for {action.actionLocationType} is null.");
            //}
            if (targetTile != null) {
                if (actor.movementComponent.isStationary) {
                    if (actor.gridTileLocation != targetTile && !actor.gridTileLocation.IsNeighbour(targetTile, true)) {
                        targetTile = null;
                    }
                }
            }
        } else if (action.actionLocationType == ACTION_LOCATION_TYPE.UPON_STRUCTURE_ARRIVAL) {
            if (actor.currentStructure == targetStructure && targetStructure.structureType != STRUCTURE_TYPE.WILDERNESS) {
                targetTile = actor.gridTileLocation;
            } else {
                //No OnArriveAtTargetLocation because it doesn't trigger on arrival on the tile itself, rather, it is triggered upon arrival on the structure
                IPointOfInterest targetToGoTo = action.GetTargetToGoTo(this);
                if (targetToGoTo == null) {
                    targetTile = action.GetTargetTileToGoTo(this);
                } else {
                    targetPOIToGoTo = targetToGoTo;
                    targetTile = targetToGoTo.gridTileLocation;
                }
                if (actor.movementComponent.isStationary) {
                    if (actor.gridTileLocation != targetTile && !actor.gridTileLocation.IsNeighbour(targetTile, true)) {
                        targetTile = null;
                    }
                }
            }
        }
    }
    private void CheckAndMoveToDoAction(JobQueueItem job) {
        if (actor.currentActionNode != this) {
            return;
        }
        if (job.originalOwner == null) {
            //If somehow job is no longer available or is destroyed when trying to move to do action, do not continue
            //This happens when job is cancelled while actor is travelling to another region
            return;
        }
        if (!MoveToDoAction(job)) {
            if (targetTile != null) {
                //If cannot move to do action because there is no path between two location grid tiles, handle it here
                if (job.originalOwner != null && job.originalOwner.ownerType != JOB_OWNER.CHARACTER) {
                    job.AddBlacklistedCharacter(actor);
                }
                actor.NoPathToDoJobOrAction(job, this);
                job.CancelJob();
            }
        } else {
            //Note: Added checking if the action has already been reset and added to pool
            //Because there is a chance that by calling the MoveToDoAction, if the actor is already in the destination tile, the PerformGoapAction will be called
            //So once, the MoveToDoAction is done processing, the actor is already done performing the said action, if that happens, then there is a chance that the job is already done also, so the action will be pooled already
            if (!hasBeenReset) {
                if (avoidCombat) {
                    if (actor.hasMarker) {
                        actor.marker.SetVisionColliderSize(CharacterManager.AVOID_COMBAT_VISION_RANGE);
                    }
                } else {
                    if (actor.hasMarker) {
                        actor.marker.SetVisionColliderSize(CharacterManager.VISION_RANGE);
                    }
                }
                action.OnMoveToDoAction(this);
            }
        }
    }
    //We only pass the job because we need to cancel it if the target tile is null
    private bool MoveToDoAction(JobQueueItem job) {
        if (targetTile == null) {
            //Here we check if there is a target tile to go to because if there is not, the target might already be destroyed/taken/disabled, if that happens, we must cancel job
            // Debug.LogWarning($"{GameManager.Instance.TodayLogString()}{actor.name} is trying to move to do action {action.goapName} with target {poiTarget.name} but target tile is null, will cancel job {job.name} instead.");
            job.CancelJob();
            return false;
        }
        Assert.IsNotNull(actor.currentRegion, $"Current region of {actor.name} is null when trying to perform {action.name} with job {job.jobType.ToString()}");
        //Only create thought bubble log when characters starts the action/moves to do the action so we can pass the target structure
        if (actor.currentRegion != targetTile.structure.region) { //different core locations
            if (actor.movementComponent.MoveToAnotherRegion(targetTile.structure.region, () => CheckAndMoveToDoAction(job)) == false || !actor.limiterComponent.canMove) {
                //character cannot exit region.
                return false;
            }
        } else {
            if (targetPOIToGoTo == null) {
                if (targetTile == actor.gridTileLocation) {
#if DEBUG_PROFILER
                    Profiler.BeginSample("Perform Goap Action 1");
#endif
                    actor.marker.StopMovement();
                    actor.PerformGoapAction();
#if DEBUG_PROFILER
                    Profiler.EndSample();
#endif
                } else {
                    if ((action.canBePerformedEvenIfPathImpossible == false && !actor.movementComponent.HasPathTo(targetTile)) || !actor.limiterComponent.canMove) {
                        return false;
                    }
#if DEBUG_PROFILER
                    Profiler.BeginSample("GoTo 1");
#endif
                    actor.marker.GoTo(targetTile, OnArriveAtTargetLocation);
#if DEBUG_PROFILER
                    Profiler.EndSample();
#endif
                }
            } else {
                if (actor.gridTileLocation == targetPOIToGoTo.gridTileLocation) {
#if DEBUG_PROFILER
                    Profiler.BeginSample("Perform Goap Action 2");
#endif
                    actor.marker.StopMovement();
                    actor.PerformGoapAction();
#if DEBUG_PROFILER
                    Profiler.EndSample();
#endif
                } else {
                    if ((action.canBePerformedEvenIfPathImpossible == false && !actor.movementComponent.HasPathTo(targetPOIToGoTo.gridTileLocation)) || !actor.limiterComponent.canMove) {
                        return false;
                    }
#if DEBUG_PROFILER
                    Profiler.BeginSample("Go To POI");
#endif
                    actor.marker.GoToPOI(targetPOIToGoTo, OnArriveAtTargetLocation);
#if DEBUG_PROFILER
                    Profiler.EndSample();
#endif
                }
            }
        }
        return true;
    }
    private void OnArriveAtTargetLocation() {
        if (hasBeenReset) {
            //Note: Added checking here that if the action is already in object pool, this should not be triggered anymore
            //This is triggered even if it is in object pool usually in CharacterMarker - OnOtherCharacterDied
            //Because when the target of an action died, the job will be cancelled before the OnOtherCharacterDied is called so when it is finally called, the action is already in object pool
            return;
        }
#if DEBUG_PROFILER
        Profiler.BeginSample($"{actor.name} - {action.name} - OnArriveAtTargetLocation");
#endif
        if (action.actionLocationType == ACTION_LOCATION_TYPE.TARGET_IN_VISION) {
            if (actor.hasMarker && actor.marker.IsPOIInVision(poiTarget)) {
                //Only do perform goap action on arrive at location if the location type is not target in vision, because if it is, we no longer need this function because perform goap action is already called upon entering vision
                actor.PerformGoapAction();
            }
        } else if (action.actionLocationType == ACTION_LOCATION_TYPE.UPON_STRUCTURE_ARRIVAL) {
            if (targetStructure != null) {
                //If action location type is Upon Structure Arrival and the character already reached the target tile but the target structure is wilderness or the target structure is not the current structure, perform again
                if (targetStructure.structureType == STRUCTURE_TYPE.WILDERNESS || actor.currentStructure != targetStructure) {
                    actor.PerformGoapAction();
                }    
            }
        } else {
            actor.PerformGoapAction();
        }
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
    public void PerformAction() {
        GoapActionInvalidity goapActionInvalidity = action.IsInvalid(this);
        string invalidVisionReason = string.Empty;
        bool isInvalidOnVision = action.IsInvalidOnVision(this, out invalidVisionReason);
        bool isInvalidStealth = IsInvalidStealth();
        if (goapActionInvalidity.isInvalid || isInvalidOnVision || isInvalidStealth) {
#if DEBUG_LOG
            Debug.Log($"{GameManager.Instance.TodayLogString()}{actor.name}'s action {action.goapType.ToString()} was invalid!\nDebug Log:\n {goapActionInvalidity.debugLog}");
#endif
            if (!string.IsNullOrEmpty(invalidVisionReason) && string.IsNullOrEmpty(goapActionInvalidity.reason)) {
                //if goap action invalidity reason is empty and invalidity reason for vision is not, then copy over value of vision invalidity, so that it will be used for the invalid log.
                goapActionInvalidity.reason = invalidVisionReason;
            }
            action.LogActionInvalid(goapActionInvalidity, this, isInvalidStealth);
            SetIsStillProcessing(true);
            actor.GoapActionResult(InteractionManager.Goap_State_Fail, this);
            action.OnInvalidAction(this);
            JobQueueItem job = associatedJob;
            if (job != null) {
                if (job.forceCancelOnInvalid || goapActionInvalidity.IsReasonForCancellationShouldDropJob()) {
                    job.ForceCancelJob();
                } else {
                    if (isInvalidOnVision || isInvalidStealth) { //If action is invalid because of stealth, cancel job immediately, we do not need to recalculate it anymore since there are witnesses around, it will just become invalid again even if we let it recalculate
                        if (job.originalOwner != null && job.originalOwner.ownerType != JOB_OWNER.CHARACTER) {
                            job.AddBlacklistedCharacter(actor);
                        }
                        job.CancelJob();
                    } else {
                        //Special case for Invite action for Make Love
                        //Once the invite action became invalid because the target rejected the invite, it must be cancelled immediately, so that the actor will not try to invite again
                        //Maybe create a system for this?
                        if (goapActionInvalidity.stateName == "Invite Rejected" && action.goapType == INTERACTION_TYPE.INVITE) {
                            if (job.originalOwner != null && job.originalOwner.ownerType != JOB_OWNER.CHARACTER) {
                                job.AddBlacklistedCharacter(actor);
                            }
                            job.CancelJob();
                        } else {
                            if (job.invalidCounter > 0) {
                                if (job.originalOwner != null && job.originalOwner.ownerType != JOB_OWNER.CHARACTER) {
                                    job.AddBlacklistedCharacter(actor);
                                }
                                job.CancelJob();
                            } else {
                                job.IncreaseInvalidCounter();
                            }
                        }
                    }
                }
            }
            SetIsStillProcessing(false);
            if (isSupposedToBeInPool) {
                ProcessReturnToPool();
            }
            return;
        }
        actionStatus = ACTION_STATUS.PERFORMING;
        actor.marker.UpdateAnimation();

        if (associatedJobType == JOB_TYPE.ENERGY_RECOVERY_NORMAL || associatedJobType == JOB_TYPE.ENERGY_RECOVERY_URGENT) {
            if (actor.gatheringComponent.hasGathering && actor.gatheringComponent.currentGathering is SocialGathering) {
                actor.gatheringComponent.currentGathering.RemoveAttendee(actor);
            }
        }

        if (poiTarget is Character targetCharacter) {
            if (!action.doesNotStopTargetCharacter && actor != poiTarget) {
                if (!targetCharacter.isDead) {
                    if (targetCharacter.marker.isMoving) {
                        targetCharacter.marker.StopMovement();
                    }
                    if (targetCharacter.stateComponent.currentState != null) {
                        targetCharacter.stateComponent.currentState.PauseState();
                    }
                    if (targetCharacter.currentActionNode != null) {
                        targetCharacter.StopCurrentActionNode();
                    }
                    targetCharacter.limiterComponent.DecreaseCanMove();
                    InnerMapManager.Instance.FaceTarget(targetCharacter, actor);
                }
                targetCharacter.AdjustNumOfActionsBeingPerformedOnThis(1);
            }
        } else {
            poiTarget.AdjustNumOfActionsBeingPerformedOnThis(1);
            if (poiTarget is TileObject targetTileObject) {
                targetTileObject.AdjustRepairCounter(1);
            }
            InnerMapManager.Instance.FaceTarget(actor, poiTarget);
        }
        if (associatedJobType != JOB_TYPE.REMOVE_STATUS && associatedJobType != JOB_TYPE.REPAIR && associatedJobType != JOB_TYPE.FEED) {
            //If self job, do not cancel
            if (actor != target) {
                poiTarget.CancelRemoveStatusFeedAndRepairJobsTargetingThis();
            }
        }
        if ((action.actionCategory == ACTION_CATEGORY.DIRECT || action.actionCategory == ACTION_CATEGORY.CONSUME) && poiTarget is BaseMapObject baseMapObject) {
            baseMapObject.OnManipulatedBy(actor);
        }
        //Note: Put a still processing here because if the action has no duration and it is the last action of the job the "action.Perform" will automatically cancel the job and return the ActualGoapNode to pool
        //When that happens, the STARTED_PERFORMING_ACTION will be broadcasting an ActualGoapNode that is already in the object pool
        SetIsStillProcessing(true);
        action.Perform(this);
        Messenger.Broadcast(JobSignals.STARTED_PERFORMING_ACTION, this);
        SetIsStillProcessing(false);
        if (isSupposedToBeInPool) {
            ProcessReturnToPool();
        }
    }
    public void ActionInterruptedWhilePerforming() { //bool shouldDoAfterEffect
                                                     //#if DEBUG_LOG
                                                     //        string log =
                                                     //            $"{GameManager.Instance.TodayLogString()}{actor.name} is interrupted while doing goap action: {action.goapName}";
                                                     //#endif
                                                     //if (shouldDoAfterEffect) {
                                                     //    string result = GoapActionStateDB.GetStateResult(action.goapType, currentState.name);
                                                     //    if (result == InteractionManager.Goap_State_Success) {
                                                     //        actionStatus = ACTION_STATUS.SUCCESS;
                                                     //    } else {
                                                     //        actionStatus = ACTION_STATUS.FAIL;
                                                     //    }    
                                                     //} else {
                                                     //    //consider action as failed since after effect was not executed.
                                                     //    actionStatus = ACTION_STATUS.FAIL;
                                                     //}
                                                     //consider action as failed since after effect was not executed.
        actionStatus = ACTION_STATUS.FAIL;

        StopPerTickEffect();
        if (poiTarget is Character targetCharacter) {
            if (!action.doesNotStopTargetCharacter && actor != poiTarget) {
                if (!targetCharacter.isDead) {
                    if (targetCharacter.stateComponent.currentState != null && targetCharacter.stateComponent.currentState.isPaused) {
                        targetCharacter.stateComponent.currentState.ResumeState();
                    }
                    targetCharacter.limiterComponent.IncreaseCanMove();
                }
                targetCharacter.AdjustNumOfActionsBeingPerformedOnThis(-1);
            }
        } else {
            poiTarget.AdjustNumOfActionsBeingPerformedOnThis(-1);
        }
        OnFinishActionTowardsTarget();
        GoapPlanJob job = actor.currentJob as GoapPlanJob;
        if (actor.currentActionNode == this) {
            actor.SetCurrentActionNode(null, null, null);
        }
        //Assert.IsNotNull(job, $"{actor.name} was interrupted when performing {action.goapName} but, in this process his/her current job is null!");
        Character p_actor = actor;
        IPointOfInterest p_target = poiTarget;
        INTERACTION_TYPE p_type = action.goapType;
        ACTION_STATUS p_status = actionStatus;

        job?.CancelJob();
        Messenger.Broadcast(JobSignals.CHARACTER_FINISHED_ACTION, p_actor, p_target, p_type, p_status);
    }
    private void ActionResult(GoapActionState actionState) {
        string result = GoapActionStateDB.GetStateResult(action.goapType, actionState.name);
        actionStatus = result == InteractionManager.Goap_State_Success ? ACTION_STATUS.SUCCESS : ACTION_STATUS.FAIL;
        StopPerTickEffect();
        if (poiTarget is Character targetCharacter) {
            if (!action.doesNotStopTargetCharacter && actor != poiTarget) {
                if (!targetCharacter.isDead) {
                    if (targetCharacter.stateComponent.currentState != null && targetCharacter.stateComponent.currentState.isPaused) {
                        targetCharacter.stateComponent.currentState.ResumeState();
                    }
                    targetCharacter.limiterComponent.IncreaseCanMove();
                }
                targetCharacter.AdjustNumOfActionsBeingPerformedOnThis(-1);
            }
        } else {
            poiTarget.AdjustNumOfActionsBeingPerformedOnThis(-1);
        }
        OnFinishActionTowardsTarget();

        Character p_actor = actor;
        IPointOfInterest p_target = poiTarget;
        INTERACTION_TYPE p_type = action.goapType;
        ACTION_STATUS p_status = actionStatus;

        actor.GoapActionResult(result, this);
        Messenger.Broadcast(JobSignals.CHARACTER_FINISHED_ACTION, p_actor, p_target, p_type, p_status);
    }
    public void StopActionNode() {
        if (actionStatus == ACTION_STATUS.PERFORMING) {
            //Toggle is processing here so that when StopActionNode is called, the currentActionNode will not be put in object pool
            SetIsStillProcessing(true);
            action.OnStopWhilePerforming(this);
            OnCancelActionTowardsTarget();
            SetIsStillProcessing(false);
#if DEBUG_LOG
            if (hasBeenReset) {
                Debug.LogError($"reset");
            }
            if (isSupposedToBeInPool) {
                Debug.LogError($"Action: {ToString()} is supposed to be in pool");
            }
#endif

            ActionInterruptedWhilePerforming();
            //if (currentState.duration == 0) { //If action has no duration then do EndPerTickEffect (this will also call the action result)
            //    //ReturnToActorTheActionResult(InteractionManager.Goap_State_Fail);
            //    EndPerTickEffect(false);
            //} else { //If action has duration and interrupted in the middle of the duration then do ActionInterruptedWhilePerforming (this will not call the action result, instead it will call the cancel job so it can be brought back to the npcSettlement list if it is a npcSettlement job)
            //    ActionInterruptedWhilePerforming();
            //}
            ////when the action is ended prematurely, make sure to readjust the target character's do not move values
            //if (poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            //    if (poiTarget != actor) {
            //        Character targetCharacter = poiTarget as Character;
            //        targetCharacter.marker.pathfindingAI.AdjustDoNotMove(-1);
            //        targetCharacter.marker.AdjustIsStoppedByOtherCharacter(-1);
            //    }
            //}
        } else if (actionStatus == ACTION_STATUS.STARTED) {
            //if (action != null && action.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            //    Character targetCharacter = action.poiTarget as Character;
            //    targetCharacter.AdjustIsWaitingForInteraction(-1);
            //}
            action.OnStopWhileStarted(this);
            //actor.DropPlan(parentPlan, forceProcessPlanJob: true); //TODO: Try to push back instead of dropping plan immediately, only drop plan if push back fails (fail: if no other plan replaces this plan)
        }
    }
    private bool IsActionStealth(JobQueueItem job) {
        if (action.goapType == INTERACTION_TYPE.REMOVE_BUFF) {
            return true;
        }
        if (action.goapType == INTERACTION_TYPE.STEAL || action.goapType == INTERACTION_TYPE.STEAL_ANYTHING
            || action.goapType == INTERACTION_TYPE.DRINK_BLOOD || action.goapType == INTERACTION_TYPE.VAMPIRIC_EMBRACE
            || action.goapType == INTERACTION_TYPE.PICKPOCKET || action.goapType == INTERACTION_TYPE.STEAL_COINS) {
            return true;
        } else if (action.goapType == INTERACTION_TYPE.KNOCKOUT_CHARACTER && job.jobType != JOB_TYPE.APPREHEND) {
            return true;
        }
        if (job.jobType == JOB_TYPE.PLACE_TRAP || job.jobType == JOB_TYPE.POISON_FOOD) {
            return true;
        }
        if (job.jobType == JOB_TYPE.SNATCH && (action.goapType == INTERACTION_TYPE.KNOCKOUT_CHARACTER || action.goapType == INTERACTION_TYPE.ASSAULT)) {
            //Snatch assault or knockout jobs must be stealth
            return true;
        }
        return false;
    }
    private bool IsActionAvoidCombat(JobQueueItem job) {
        if (job.jobType == JOB_TYPE.STEAL_CORPSE) {
            return true;
        }
        return false;
    }

    //Right now this is only used on rumors, since rumors are just illusion action (meaning, the actor did not really do it), the target structure for rumors are always null
    //So if we need the target structure in the logs, no target structure will be filled.
    //That is why we must call this on the rumors that needs a target structure like Poison action
    public void SetTargetStructure(LocationStructure structure) {
        targetStructure = structure;
    }
    #endregion

    #region Action State
    public void OnActionStateSet(string stateName) {
#if DEBUG_LOG
        Debug.Log($"Set action state of {actor.name}'s {action.goapName} to {stateName}");
#endif
        currentStateName = stateName;
        OnPerformActualActionToTarget();
        ExecuteCurrentActionState();
    }
    private void ExecuteCurrentActionState() {
#if DEBUG_LOG
        Debug.Log($"Executing action state of {actor.name}'s {action.goapName}, {currentStateName}");
#endif
        GoapActionState state = currentState;

        IPointOfInterest target = poiTarget;
        //if(poiTarget is TileObject && action.goapType == INTERACTION_TYPE.STEAL) {
        //    TileObject item = poiTarget as TileObject;
        //    if(item.isBeingCarriedBy != null) {
        //        target = item.isBeingCarriedBy;
        //    }
        //}

        if (isStealth && target.traitContainer.HasTrait("Vigilant") && target.traitContainer.HasTrait(InteractionManager.Instance.vigilantCancellingTraits) == false && !target.isDead) {
            //trigger vigilant, only if character is NOT resting or unconscious
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "vigilant", this, LOG_TAG.Social);
            action.AddFillersToLog(log, this);
            log.AddToFillers(null, action.name, LOG_IDENTIFIER.STRING_1);
            OverrideDescriptionLog(log);
            actor.marker.UpdateAnimation();

            //When a character is vigilant and an action has started performing do not let it perform the action even if the action has duration
            //Go to End Effect immediately
            //Because we do not want the actor to wait standing if the target is vigilant, the reaction should be instantaneous
            //So we need to bypass the duration and assume that the action will end already
            if (state.duration != -1) {
                currentStateDuration = state.duration;
                EndPerTickEffect();
            }

        } else {
            CreateDescriptionLog(state);
            state.preEffect?.Invoke(this);
            List<Trait> actorTraitOverrideFunctions = actor.traitContainer.GetTraitOverrideFunctions(TraitManager.Execute_Pre_Effect_Trait);
            List<Trait> targetTraitOverrideFunctions = poiTarget.traitContainer.GetTraitOverrideFunctions(TraitManager.Execute_Pre_Effect_Trait);
            if (actorTraitOverrideFunctions != null) {
                for (int i = 0; i < actorTraitOverrideFunctions.Count; i++) {
                    Trait trait = actorTraitOverrideFunctions[i];
                    trait.ExecuteActionPreEffects(action.goapType, this);
                }
            }
            if (targetTraitOverrideFunctions != null) {
                for (int i = 0; i < targetTraitOverrideFunctions.Count; i++) {
                    Trait trait = targetTraitOverrideFunctions[i];
                    trait.ExecuteActionPreEffects(action.goapType, this);
                }
            }

            actor.marker.UpdateAnimation();
            //parentAction.SetExecutionDate(GameManager.Instance.Today());

            if (state.duration > 0) {
                currentStateDuration = 0;
                StartPerTickEffect();
            } else if (state.duration != -1) {
                EndPerTickEffect();
            }
        }


    }
    private void StartPerTickEffect() {
        //#if DEBUG_LOG
        //        Debug.Log("Started per tick effect: " + ToString());
        //#endif
        hasStartedPerTickEffect = true;
        //Messenger.AddListener(Signals.TICK_STARTED, PerTickEffect);
    }
    public void StopPerTickEffect() {
        //#if DEBUG_LOG
        //        Debug.Log("Stopped per tick effect: " + ToString());
        //#endif
        //Messenger.RemoveListener(Signals.TICK_STARTED, PerTickEffect);
        hasStartedPerTickEffect = false;
    }
    public void EndPerTickEffect(bool shouldDoAfterEffect = true) {
        //if (isDone) {
        //    return;
        //}
        //isDone = true;
        // Debug.Log("Executing end per tick effect of " + actor.name + "'s " + action.goapName + ", " + currentStateName + ". Action status is " + actionStatus.ToString());
        if (actionStatus == ACTION_STATUS.FAIL || actionStatus == ACTION_STATUS.SUCCESS) { //This means that the action is already finished
            return;
        }
        //Separate calls for end effect if target is vigilang and the action is stealth because there are things that will be called in normal effect that does not apply to vigilant
        if (isStealth && target.traitContainer.HasTrait("Vigilant") && !target.traitContainer.HasTrait(InteractionManager.Instance.vigilantCancellingTraits) && !target.isDead) {
            EndEffectVigilant();
        } else {
            EndEffectNormal(shouldDoAfterEffect);
        }
    }

    public void LogAction(Log p_log, bool ignoreShouldAddLog = false) {
        if (p_log != null && (action.shouldAddLogs || ignoreShouldAddLog) && CharacterManager.Instance.CanAddCharacterLogOrShowNotif(action.goapType)) { //only add logs if both the parent action and this state should add logs
            p_log.AddLogToDatabase();
            //Only show notif if an action can be stored as an intel to reduce notifications and info overload to the player
            if (action.ShouldActionBeAnIntel(this)) {
                bool cannotBeStoredAsIntel = !actor.isNormalCharacter && (!(poiTarget is Character) || !(poiTarget as Character).isNormalCharacter);
                if (!cannotBeStoredAsIntel) {
                    PlayerManager.Instance.player.ShowNotificationFrom(actor, InteractionManager.Instance.CreateNewIntel(this));
                }
            } else if (action.goapType == INTERACTION_TYPE.EXPLORE || action.goapType == INTERACTION_TYPE.COUNTERATTACK_ACTION
                || action.goapType == INTERACTION_TYPE.EXTERMINATE || action.goapType == INTERACTION_TYPE.HUNT_HEIRLOOM || action.goapType == INTERACTION_TYPE.RAID
                || action.goapType == INTERACTION_TYPE.RESCUE || action.goapType == INTERACTION_TYPE.HOST_SOCIAL_PARTY || action.goapType == INTERACTION_TYPE.JUDGE_CHARACTER
                || action.goapType == INTERACTION_TYPE.NEUTRALIZE) {
                PlayerManager.Instance.player.ShowNotificationFromPlayer(p_log);
            } else if (action.showNotification) {
                PlayerManager.Instance.player.ShowNotificationFrom(actor, p_log);
            }
        }
    }
    private void EndEffectNormal(bool shouldDoAfterEffect) {
        if (shouldDoAfterEffect) {
            LogAction(descriptionLog);
        }
        GoapActionState state = currentState;
        Character p_actor = actor;
        IPointOfInterest p_target = poiTarget;
        GoapAction p_action = action;
        SetIsStillProcessing(true);

        ActionResult(state);

        //After effect and logs should be done after processing action result so that we can be sure that the action is completely done before doing anything
        if (shouldDoAfterEffect) { // && !(isStealth && target.traitContainer.HasTrait("Vigilant"))
            state.afterEffect?.Invoke(this);
            bool isRemoved = false;
            List<Trait> actorTraitOverrideFunctions = p_actor.traitContainer.GetTraitOverrideFunctions(TraitManager.Execute_After_Effect_Trait);
            List<Trait> targetTraitOverrideFunctions = p_target.traitContainer.GetTraitOverrideFunctions(TraitManager.Execute_After_Effect_Trait);
            if (actorTraitOverrideFunctions != null) {
                for (int i = 0; i < actorTraitOverrideFunctions.Count; i++) {
                    Trait trait = actorTraitOverrideFunctions[i];
                    isRemoved = false;
                    trait.ExecuteActionAfterEffects(p_action.goapType, p_actor, p_target, p_action.actionCategory, ref isRemoved);
                    if (isRemoved) { i--; }
                }
            }
            if (targetTraitOverrideFunctions != null) {
                for (int i = 0; i < targetTraitOverrideFunctions.Count; i++) {
                    Trait trait = targetTraitOverrideFunctions[i];
                    isRemoved = false;
                    trait.ExecuteActionAfterEffects(p_action.goapType, p_actor, p_target, p_action.actionCategory, ref isRemoved);
                    if (isRemoved) { i--; }
                }
            }
        }
        SetIsStillProcessing(false);
        if (isSupposedToBeInPool) {
            ProcessReturnToPool();
        }
    }
    private void EndEffectVigilant() {
        if (descriptionLog != null && action.shouldAddLogs && CharacterManager.Instance.CanAddCharacterLogOrShowNotif(action.goapType)) { //only add logs if both the parent action and this state should add logs
            descriptionLog.AddLogToDatabase();
            PlayerManager.Instance.player.ShowNotificationFrom(actor.gridTileLocation, descriptionLog);
        }
        JobQueueItem currentJob = actor.currentJob;
        //Result of the action will be "successful" but only in writing
        //It's as if the action is successful but in reality it is not
        //The reason for this is for the action to go through proper flow in ActionResult, so that action done will not be broken and all action detachments and unassigning will be done
        GoapActionState state = currentState;
        ActionResult(state);

        //If there is still job after processing results, we need to cancel it here because if the target is vigilant the actio has failed in reality, so the job must be cancelled
        if (currentJob != null && currentJob.jobType != JOB_TYPE.NONE && !currentJob.hasBeenReset) {
            //Checking if job type must not be none, because if it is none, the job is not used anymore
            currentJob.CancelJob();
        }
    }
    public void PerTickEffect() {
        if (hasBeenReset) {
#if DEBUG_LOG
            Debug.Log("Per Tick Effect called but already in pool");
#endif
            StopPerTickEffect();
            return;
        }
        if (!hasStartedPerTickEffect) {
#if DEBUG_LOG
            Debug.Log("Per Tick Effect called but not started yet");
#endif
            return;
        }

#if DEBUG_PROFILER
        Profiler.BeginSample($"{actor.name} - {action.name} - Per Tick Effect");
#endif
        
        GoapActionState state = currentState;
        currentStateDuration++;

        IPointOfInterest target = poiTarget;
        //if(poiTarget is TileObject && action.goapType == INTERACTION_TYPE.STEAL) {
        //    TileObject item = poiTarget as TileObject;
        //    if(item.isBeingCarriedBy != null) {
        //        target = item.isBeingCarriedBy;
        //    }
        //}

        if (!actor.interruptComponent.hasTriggeredSimultaneousInterrupt) {
            InnerMapManager.Instance.FaceTarget(actor, target);
        }

        //if (!(isStealth && target.traitContainer.HasTrait("Vigilant"))) {
        state.perTickEffect?.Invoke(this);

        //Once per tick effect is called, check again if the action has already been reset to pool, because there are actions that cancels job in per tick effect
        //So, if that happens, the part below should no longer trigger
        if (hasBeenReset) {
            StopPerTickEffect();
            return;
        }

        List<Trait> actorTraitOverrideFunctions = actor.traitContainer.GetTraitOverrideFunctions(TraitManager.Execute_Per_Tick_Effect_Trait);
        List<Trait> targetTraitOverrideFunctions = poiTarget.traitContainer.GetTraitOverrideFunctions(TraitManager.Execute_Per_Tick_Effect_Trait);
        if (actorTraitOverrideFunctions != null) {
            for (int i = 0; i < actorTraitOverrideFunctions.Count; i++) {
                Trait trait = actorTraitOverrideFunctions[i];
                trait.ExecuteActionPerTickEffects(action.goapType, this);
            }
        }
        if (targetTraitOverrideFunctions != null) {
            for (int i = 0; i < targetTraitOverrideFunctions.Count; i++) {
                Trait trait = targetTraitOverrideFunctions[i];
                trait.ExecuteActionPerTickEffects(action.goapType, this);
            }
        }
        //}
        if (currentStateDuration >= state.duration) {
#if DEBUG_PROFILER
            Profiler.BeginSample($"{actor.name} - {action.name} - End Per Tick Effect");
#endif
            EndPerTickEffect();
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif
        }
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
    private void OnPerformActualActionToTarget() {
        if (GoapActionStateDB.GetStateResult(action.goapType, currentStateName) != InteractionManager.Goap_State_Success) {
            return;
        }
        if (poiTarget is TileObject tileObject) {
            tileObject.OnDoActionToObject(this);
        }
        //else if (poiTarget is Character) {
        //    if (currentState.name != "Target Missing" && !doesNotStopTargetCharacter) {
        //        AddAwareCharacter(poiTarget as Character);
        //    }
        //}
    }
    private void OnFinishActionTowardsTarget() {
        if (poiTarget is TileObject targetTileObject) {
            targetTileObject.AdjustRepairCounter(-1);
            if (actionStatus != ACTION_STATUS.FAIL) {
                targetTileObject.OnDoneActionToObject(this);
            }
        }
    }
    private void OnCancelActionTowardsTarget() {
        if (poiTarget is TileObject tileObject) {
            tileObject.OnCancelActionTowardsObject(this);
        }
    }
    public void OverrideCurrentStateDuration(int val) {
        currentStateDuration = val;
    }
    #endregion

    #region Log
    private void CreateDescriptionLog(GoapActionState actionState) {
        if (descriptionLog == null) {
            descriptionLog = actionState.CreateDescriptionLog(this);
        }
    }
    private void CreateThoughtBubbleLog() {
        if (thoughtBubbleLog == null) {
            if (LocalizationManager.Instance.HasLocalizedValue("GoapAction", action.goapName, "thought_bubble")) {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", action.goapName, "thought_bubble", this);
                action.AddFillersToLog(log, this);
                thoughtBubbleLog = log;
            }
        }
        if (thoughtBubbleMovingLog == null) {
            if (LocalizationManager.Instance.HasLocalizedValue("GoapAction", action.goapName, "thought_bubble_m")) {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", action.goapName, "thought_bubble_m", this);
                action.AddFillersToLog(log, this);
                thoughtBubbleMovingLog = log;
            }
        }
    }
    public Log GetCurrentLog() {
        if (actionStatus == ACTION_STATUS.STARTED) {
            return thoughtBubbleMovingLog;
        } else if (actionStatus == ACTION_STATUS.PERFORMING) {
            if (thoughtBubbleLog == null) {
                //This is to prevent errors for logs without thought bubble in json file
                //Bug: https://trello.com/c/eC0OQh3N/4275-charactervisualsgetthoughtbubble
                return thoughtBubbleMovingLog;
            }
            return thoughtBubbleLog;
        }
        return descriptionLog;
    }
    public void OverrideDescriptionLog(Log log) {
        if (descriptionLog != null) {
            LogPool.Release(descriptionLog); //release old description log if any
        }
        descriptionLog = log;
    }
    public string StringText() {
        return $"{action.goapName} with actor => {actor.name}, and target => {poiTarget.name}";
    }
    private void SetDefaultLogTags() {
        logTags.Clear();
        if (action.logTags != null) {
            for (int i = 0; i < action.logTags.Length; i++) {
                logTags.Add(action.logTags[i]);
            }
        }
    }
    private void SetAdditionalLogTags() {
        if (action.ShouldActionBeAnIntel(this)) {
            if (!logTags.Contains(LOG_TAG.Intel)) {
                logTags.Add(LOG_TAG.Intel);
            }
        }
    }
    #endregion

    #region Jobs
    public void OnAttachPlanToJob(GoapPlanJob job) {
        isStealth = job.isStealth;
    }
    public void OnUnattachPlanToJob(GoapPlanJob job) {
        if (associatedJob == job) {
            SetJob(null);
        }
    }
    public void SetJob(JobQueueItem job) {
        associatedJob = job;
    }
    //private JobQueueItem GetAssociatedJob () {
    //    if(_associatedJob != null && _associatedJob.originalOwner) {

    //    }
    //}
    #endregion

    #region Character
    public void AddAwareCharacter(Character character) {
        awareCharacters.Add(character);
    }
    #endregion

    #region General
    public override string ToString() {
        return $"Action: {action?.name ?? "Null"}. Actor: {actor?.name} . Target: {poiTarget?.name ?? "Null"}";
    }
    #endregion

    #region IRumorable
    public void SetAsRumor(Rumor newRumor) {
        if (rumor != newRumor) {
            rumor = newRumor;
            if (rumor != null) {
                rumor.SetRumorable(this);
                actionStatus = ACTION_STATUS.SUCCESS;
                currentStateName = GoapActionStateDB.goapActionStates[goapType][0].name;
                CreateDescriptionLog(currentState);
            }
        }
    }
    #endregion

    #region Assumption
    public void SetAsAssumption(Assumption newAssumption) {
        if (assumption != newAssumption) {
            assumption = newAssumption;
            if (assumption != null) {
                assumption.SetAssumedAction(this);
                actionStatus = ACTION_STATUS.SUCCESS;
                currentStateName = GoapActionStateDB.goapActionStates[goapType][0].name;
                CreateDescriptionLog(currentState);
            }
        }
    }
    #endregion

    #region Assumption
    public void SetIsIntel(bool p_state) {
        isIntel = p_state;
    }
    public void SetIsNegativeInfo(bool p_state) {
        isNegativeInfo = p_state;
    }
    #endregion

    #region Illusion
    //Illusion actions are actions that are not really performed by the actor physically but is perceived by the witnesses that is was performed
    //Example: Trespassing
    public void SetAsIllusion() {
        actionStatus = ACTION_STATUS.SUCCESS;
        currentStateName = GoapActionStateDB.goapActionStates[goapType][0].name;
        CreateDescriptionLog(currentState);
    }
    #endregion

    #region IReactable
    public string ReactionToActor(Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status) {
        return action.ReactionToActor(actor, target, witness, this, status);
    }
    public string ReactionToTarget(Character actor, IPointOfInterest target, Character witness,
        REACTION_STATUS status) {
        return action.ReactionToTarget(actor, target, witness, this, status);
    }
    public string ReactionOfTarget(Character actor, IPointOfInterest target, REACTION_STATUS status) {
        return action.ReactionOfTarget(actor, target, this, status);
    }
    public void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status) {
        action.PopulateEmotionReactionsToActor(reactions, actor, target, witness, this, status);
    }
    public void PopulateReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status) {
        action.PopulateEmotionReactionsToTarget(reactions, actor, target, witness, this, status);
    }
    public void PopulateReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, REACTION_STATUS status) {
        action.PopulateEmotionReactionsOfTarget(reactions, actor, target, this, status);
    }
    public REACTABLE_EFFECT GetReactableEffect(Character witness) {
        return action.GetReactableEffect(this, witness);
    }
    #endregion

    #region Crime
    public void SetCrimeType() {
        if (crimeType == CRIME_TYPE.Unset) {
            Character actor = this.actor;
            IPointOfInterest target = poiTarget;
            if (this.actor.reactionComponent.disguisedCharacter != null) {
                actor = this.actor.reactionComponent.disguisedCharacter;
            }
            if (poiTarget is Character targetCharacter && targetCharacter.reactionComponent.disguisedCharacter != null) {
                target = targetCharacter.reactionComponent.disguisedCharacter;
            }
            crimeType = action.GetCrimeType(actor, target, this);
            if (crimeType != CRIME_TYPE.None) {
                if (!logTags.Contains(LOG_TAG.Crimes)) {
                    logTags.Add(LOG_TAG.Crimes);
                }
            }
            SetAdditionalLogTags();
        }
    }
    #endregion

    #region Unique Action Data
    private UniqueActionData CreateUniqueActionData(GoapAction action) {
        if (action.uniqueActionDataType != null) {
            UniqueActionData data = System.Activator.CreateInstance(action.uniqueActionDataType) as UniqueActionData;
            Assert.IsNotNull(data, $"{action.goapType.ToString()} has provided a unique action data type {action.uniqueActionDataType} but no class like that exists!");
            return data;
        }
        return null;
    }
    public T GetConvertedUniqueActionData<T>() where T : UniqueActionData {
        T converted = uniqueActionData as T;
        Assert.IsNotNull(converted, $"Trying to get converted unique action data of {action.goapName} of actor {actor.name} but it could not be converted! Unique action data value is {uniqueActionData}");
        return converted;
    }
    #endregion

    #region Stealth
    private bool IsInvalidStealth() {
        //If action is stealth and there is a character in vision that can witness and considers the action as a crime, then return false, this means that the actor must not do the action because there are witnesses
        //Only do this if the actor is a Villager, otherwise, it does not make sense for monsters to be stealthy
        if (poiTarget != actor && isStealth && actor.isNormalCharacter) {
            IPointOfInterest trueTarget = poiTarget;
            //If action is steal, we must check the carrier of the item (poiTarget) that is being stolen, instead of the item itself
            //if (action.goapType == INTERACTION_TYPE.STEAL) {
            //    if (poiTarget.isBeingCarriedBy != null) {
            //        trueTarget = poiTarget.isBeingCarriedBy;
            //    }
            //}
            if (actor.marker && actor.marker.IsPOIInVision(trueTarget) && !actor.marker.CanDoStealthCrimeToTarget(trueTarget, crimeType)) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Loading
    public void DoActionUponLoadingSavedGame() {
        if (actionStatus == ACTION_STATUS.STARTED) {
            //TODO: Resume doing action
            actor.SetCurrentActionNode(null, null, null);
            //CheckAndMoveToDoAction(associatedJob);
        } else if (actionStatus == ACTION_STATUS.PERFORMING) {
            actor.marker.UpdateAnimation();
            if (currentState.duration > 0) {
                StartPerTickEffect();
            } else if (currentState.duration != -1) {
                EndPerTickEffect();
            }
        } else {
            throw new System.Exception("Action " + action.name + " of " + actor.name + " is being done again after loading but the status is " + actionStatus.ToString());
        }
    }
    public bool LoadReferences(SaveDataActualGoapNode data) {
        bool isViable = true;
        actor = CharacterManager.Instance.GetCharacterByPersistentID(data.actor);
        if (actor == null) {
            isViable = false;
        }
        if (data.poiTargetType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            poiTarget = CharacterManager.Instance.GetCharacterByPersistentID(data.poiTarget);
            if (poiTarget == null) {
                isViable = false;
            }
        } else if (data.poiTargetType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
            poiTarget = InnerMapManager.Instance.GetTileObjectByPersistentID(data.poiTarget);
            if (poiTarget == null) {
                isViable = false;
            }
        }
        if (!string.IsNullOrEmpty(data.disguisedActor)) {
            disguisedActor = CharacterManager.Instance.GetCharacterByPersistentID(data.disguisedActor);
        }
        if (!string.IsNullOrEmpty(data.disguisedTarget)) {
            disguisedTarget = CharacterManager.Instance.GetCharacterByPersistentID(data.disguisedTarget);
        }
        // if (!string.IsNullOrEmpty(data.thoughtBubbleLog)) {
        //     thoughtBubbleLog = DatabaseManager.Instance.logDatabase.GetLogByPersistentID(data.thoughtBubbleLog);
        // }
        // if (!string.IsNullOrEmpty(data.thoughtBubbleMovingLog)) {
        //     thoughtBubbleMovingLog = DatabaseManager.Instance.logDatabase.GetLogByPersistentID(data.thoughtBubbleMovingLog);
        // }
        // if (!string.IsNullOrEmpty(data.descriptionLog)) {
        //     descriptionLog = DatabaseManager.Instance.logDatabase.GetLogByPersistentID(data.descriptionLog);
        // }
        if (data.thoughtBubbleLog != null) {
            thoughtBubbleLog = data.thoughtBubbleLog;
        }
        if (data.thoughtBubbleMovingLog != null) {
            thoughtBubbleMovingLog = data.thoughtBubbleMovingLog;
        }
        if (data.descriptionLog != null) {
            descriptionLog = data.descriptionLog;
        }
        if (!string.IsNullOrEmpty(data.targetStructure)) {
            targetStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(data.targetStructure);
        }
        if (!string.IsNullOrEmpty(data.targetPOIToGoTo)) {
            if (data.targetPOIToGoToType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                targetPOIToGoTo = CharacterManager.Instance.GetCharacterByPersistentID(data.targetPOIToGoTo);
            } else if (data.targetPOIToGoToType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                targetPOIToGoTo = InnerMapManager.Instance.GetTileObjectByPersistentID(data.targetPOIToGoTo);
            }
            if (targetPOIToGoTo == null) {
                isViable = false;
            }
        }
        if (data.awareCharacters != null) {
            for (int i = 0; i < data.awareCharacters.Count; i++) {
                Character character = CharacterManager.Instance.GetCharacterByPersistentID(data.awareCharacters[i]);
                if (character != null) {
                    awareCharacters.Add(character);
                }
            }
        }
        if (data.hasRumor) {
            rumor = data.rumor.Load();
            rumor.SetRumorable(this);
        }
        if (data.hasAssumption) {
            assumption = data.assumption.Load();
            assumption.SetAssumedAction(this);
        }
        if (data.otherData != null) {
            otherData = new OtherData[data.otherData.Length];
            for (int i = 0; i < otherData.Length; i++) {
                SaveDataOtherData d = data.otherData[i];
                if (d != null) {
                    otherData[i] = d.Load();
                }
            }
        }
        if (!string.IsNullOrEmpty(data.associatedJobID)) {
            associatedJob = DatabaseManager.Instance.jobDatabase.GetJobWithPersistentID(data.associatedJobID);
        }
        if (data.uniqueActionData != null) {
            uniqueActionData = data.uniqueActionData.Load();
        }
        return isViable;
    }
    /// <summary>
    /// Load other references, because there are some components in the action that depends on other actions being fully loaded
    /// i.e RumorOtherData that needs to have had all actions rumor data to be loaded since it will reference it.
    /// </summary>
    /// <param name="data">The save data of this action.</param>
    public void LoadAdditionalReferences(SaveDataActualGoapNode data) {
        if (otherData != null) {
            for (int i = 0; i < otherData.Length; i++) {
                OtherData d = otherData[i];
                SaveDataOtherData saveDataOtherData = data.otherData.ElementAtOrDefault(i);
                if (d != null && saveDataOtherData != null) {
                    d.LoadAdditionalData(saveDataOtherData);
                }
            }
        }
    }
    #endregion

    #region Operators
    public static bool operator ==(ActualGoapNode left, ActualGoapNode right) {
        return left?.persistentID == right?.persistentID;
    }
    public static bool operator !=(ActualGoapNode left, ActualGoapNode right) {
        return left?.persistentID != right?.persistentID;
    }
    public static bool operator ==(ActualGoapNode left, ICrimeable right) {
        return left?.persistentID == right?.persistentID;
    }
    public static bool operator !=(ActualGoapNode left, ICrimeable right) {
        return left?.persistentID != right?.persistentID;
    }
    public static bool operator ==(ActualGoapNode left, IRumorable right) {
        return left?.persistentID == right?.persistentID;
    }
    public static bool operator !=(ActualGoapNode left, IRumorable right) {
        return left?.persistentID != right?.persistentID;
    }
    public override bool Equals(object obj) {
        if (obj is ActualGoapNode to) {
            return persistentID.Equals(to.persistentID);
        }
        return false;
    }
    public override int GetHashCode() {
        return base.GetHashCode();
    }
    #endregion

    #region Object Pool
    public void IncreaseAssignmentCounter() {

    }
    public void SetIsStillProcessing(bool p_state) {
        if (p_state) {
            stillProcessingCounter++;
        } else {
            stillProcessingCounter--;
        }
    }
    public void SetIsSupposedToBeInPool(bool p_state) {
        isSupposedToBeInPool = p_state;
    }
    public void SetHasBeenReset(bool p_state) {
        hasBeenReset = p_state;
    }
    public void IncreaseReactionCounter() {
        reactionProcessCounter++;
    }
    public void DecreaseReactionCounter() {
        reactionProcessCounter--;
        if (isSupposedToBeInPool) {
            ProcessReturnToPool();
        }
    }
    public bool ProcessReturnToPool() {
        if (reactionProcessCounter <= 0) {
            if (!isRumor && !isIntel && !isAssumption && !isStillProcessing && !isNegativeInfo) {
                ObjectPoolManager.Instance.ReturnActionToPool(this);
                return true;
            }
        }
        return false;
    }
    public void Reset() {
        persistentID = null; //string.Empty //Note: Null this instead of string.Empty so that the == and != operators will be accurate when checking for nulls since we override it to check for persistent id instead of the instance of the class itself
        actor = null;
        poiTarget = null;
        disguisedActor = null;
        disguisedTarget = null;
        isStealth = false;
        avoidCombat = false;
        otherData = null;
        cost = 0;
        action = null;
        thoughtBubbleLog = null;
        thoughtBubbleMovingLog = null;
        descriptionLog = null;
        targetStructure = null;
        targetTile = null;
        targetPOIToGoTo = null;
        associatedJobType = JOB_TYPE.NONE;
        associatedJob = null;

        currentStateName = string.Empty;
        currentStateDuration = 0;
        rumor = null;
        assumption = null;

        awareCharacters.Clear();
        logTags.Clear();

        //Crime
        crimeType = CRIME_TYPE.Unset;
        uniqueActionData = null;
        actionStatus = ACTION_STATUS.NONE;
        isSupposedToBeInPool = false;
        isIntel = false;
        isNegativeInfo = false;
        stillProcessingCounter = 0;
        SetHasBeenReset(true);
        StopPerTickEffect();
    }
    #endregion
}

[System.Serializable]
public class SaveDataActualGoapNode : SaveData<ActualGoapNode>, ISavableCounterpart {
    public string persistentID { get; set; }
    public string actor;
    public string poiTarget;
    public POINT_OF_INTEREST_TYPE poiTargetType;
    public string disguisedActor;
    public string disguisedTarget;
    public bool isStealth;
    public bool avoidCombat;
    public SaveDataOtherData[] otherData;
    public int cost;
    public bool isIntel;
    public bool isNegativeInfo;
    public bool isSupposedToBeInPool;
    public int stillProcessingCounter;
    public bool hasBeenReset;
    public bool isAssigned;
    public bool hasStartedPerTickEffect;

    public INTERACTION_TYPE action;
    public ACTION_STATUS actionStatus;
    public Log thoughtBubbleLog;
    public Log thoughtBubbleMovingLog;
    public Log descriptionLog;
    public string targetStructure;
    public string targetTile;
    public string targetPOIToGoTo;
    public POINT_OF_INTEREST_TYPE targetPOIToGoToType;
    public JOB_TYPE associatedJobType;
    public string associatedJobID;

    public string currentStateName;
    public int currentStateDuration;
    public SaveDataRumor rumor;
    public SaveDataAssumption assumption;
    public bool hasRumor;
    public bool hasAssumption;

    public List<string> awareCharacters;
    public List<LOG_TAG> logTags;

    //Crime
    public CRIME_TYPE crimeType;

    public SaveDataUniqueActionData uniqueActionData;

    #region getters
    public OBJECT_TYPE objectType => OBJECT_TYPE.Action;
    #endregion

    #region Overrides
    public override void Save(ActualGoapNode data) {
        persistentID = data.persistentID;
        isStealth = data.isStealth;
        avoidCombat = data.avoidCombat;
        cost = data.cost;
        action = data.action.goapType;
        actionStatus = data.actionStatus;
        associatedJobType = data.associatedJobType;
        currentStateName = data.currentStateName;
        currentStateDuration = data.currentStateDuration;
        crimeType = data.crimeType;

        actor = data.actor.persistentID;
        poiTarget = data.poiTarget.persistentID;
        poiTargetType = data.poiTarget.poiType;
        isIntel = data.isIntel;
        isNegativeInfo = data.isNegativeInfo;
        isSupposedToBeInPool = data.isSupposedToBeInPool;
        stillProcessingCounter = data.stillProcessingCounter;
        hasBeenReset = data.hasBeenReset;
        logTags = data.logTags;
        hasStartedPerTickEffect = data.hasStartedPerTickEffect;

        disguisedActor = string.Empty;
        disguisedTarget = string.Empty;
        if (data.disguisedActor != null) {
            disguisedActor = data.disguisedActor.persistentID;
        }
        if (data.disguisedTarget != null) {
            disguisedTarget = data.disguisedTarget.persistentID;
        }

        // if (data.thoughtBubbleLog != null) {
        //     thoughtBubbleLog = data.thoughtBubbleLog.persistentID;
        //     SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.thoughtBubbleLog);
        // }
        // if (data.thoughtBubbleMovingLog != null) {
        //     thoughtBubbleMovingLog = data.thoughtBubbleMovingLog.persistentID;
        //     SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.thoughtBubbleMovingLog);
        // }
        // if (data.descriptionLog != null) {
        //     descriptionLog = data.descriptionLog.persistentID;
        //     SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.descriptionLog);
        // }
        if (data.thoughtBubbleLog != null) {
            thoughtBubbleLog = data.thoughtBubbleLog;
        }
        if (data.thoughtBubbleMovingLog != null) {
            thoughtBubbleMovingLog = data.thoughtBubbleMovingLog;
        }
        if (data.descriptionLog != null) {
            descriptionLog = data.descriptionLog;
        }
        if (data.targetStructure != null) {
            targetStructure = data.targetStructure.persistentID;
        }
        if (data.targetStructure != null) {
            targetStructure = data.targetStructure.persistentID;
        }
        if (data.targetPOIToGoTo != null) {
            targetPOIToGoTo = data.targetPOIToGoTo.persistentID;
            targetPOIToGoToType = data.targetPOIToGoTo.poiType;
        }
        awareCharacters = new List<string>();
        if (data.awareCharacters != null && data.awareCharacters.Count > 0) {
            for (int i = 0; i < data.awareCharacters.Count; i++) {
                Character character = data.awareCharacters[i];
                if (character == null) {
                    //If character is null remove it from the list
                    data.awareCharacters.RemoveAt(i);
                    i--;
                    continue;
                }
                awareCharacters.Add(character.persistentID);
            }
        }
        if (data.rumor != null) {
            hasRumor = true;
            rumor = new SaveDataRumor();
            rumor.Save(data.rumor);
        }
        if (data.assumption != null) {
            hasAssumption = true;
            assumption = new SaveDataAssumption();
            assumption.Save(data.assumption);
        }
        if (data.otherData != null) {
            otherData = new SaveDataOtherData[data.otherData.Length];
            for (int i = 0; i < otherData.Length; i++) {
                OtherData d = data.otherData[i];
                if (d != null) {
                    otherData[i] = d.Save();
                }
            }
        }
        if (data.associatedJob != null && data.associatedJob.jobType != JOB_TYPE.NONE) {
            associatedJobID = data.associatedJob.persistentID;
        }
        if (data.uniqueActionData != null) {
            uniqueActionData = data.uniqueActionData.Save();
        }
        isAssigned = data.isAssigned;
    }

    public override ActualGoapNode Load() {
        ActualGoapNode action = new ActualGoapNode(this);
        return action;
    }
    #endregion
}