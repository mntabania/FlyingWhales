using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using Traits;
using UnityEngine.Assertions;

public class GoapNode {
    //public GoapNode parent;
    //public int index;
    public int cost;
    public int level;
    public GoapAction action;
    public IPointOfInterest target;

    public void Initialize(int cost, int level, GoapAction action, IPointOfInterest target) {
        this.cost = cost;
        this.level = level;
        this.action = action;
        this.target = target;
    }

    public void Reset() {
        this.cost = 0;
        this.level = 0;
        this.action = null;
        this.target = null;
    }
}
public class MultiJobNode : JobNode{
    public override ActualGoapNode singleNode { get { return null; } }
    public override ActualGoapNode[] multiNode { get { return nodes; } }
    public override int currentNodeIndex { get { return currentIndex; } }

    private ActualGoapNode[] nodes;
    private int currentIndex;
    public MultiJobNode(ActualGoapNode[] nodes) {
        this.nodes = nodes;
        currentIndex = 0;
    }

    #region Overrides
    public override void OnAttachPlanToJob(GoapPlanJob job) {
        for (int i = 0; i < nodes.Length; i++) {
            nodes[i].OnAttachPlanToJob(job);
        }
    }
    public override void SetNextActualNode() {
        currentIndex += 1;
    }
    public override bool IsCurrentActionNode(ActualGoapNode node) {
        for (int i = 0; i < nodes.Length; i++) {
            ActualGoapNode currNode = nodes[i];
            if(currNode == node) {
                return true;
            }
        }
        return false;
    }
    #endregion
}
public class SingleJobNode : JobNode {
    public override ActualGoapNode singleNode { get { return node; } }
    public override ActualGoapNode[] multiNode { get { return null;} }
    public override int currentNodeIndex { get { return -1; } }

    private ActualGoapNode node;
    public SingleJobNode(ActualGoapNode node) {
        this.node = node;
    }

    #region Overrides
    public override void OnAttachPlanToJob(GoapPlanJob job) {
        node.OnAttachPlanToJob(job);
    }
    public override void SetNextActualNode() {
        //Not Applicable
    }
    public override bool IsCurrentActionNode(ActualGoapNode node) {
        return this.node == node;
    }
    #endregion
}
public abstract class JobNode {
    public abstract ActualGoapNode singleNode { get; }
    public abstract ActualGoapNode[] multiNode { get; }
    public abstract int currentNodeIndex { get; }
    public abstract void OnAttachPlanToJob(GoapPlanJob job);
    public abstract void SetNextActualNode();
    public abstract bool IsCurrentActionNode(ActualGoapNode node);
}

//actual nodes located in a finished plan that is going to be executed by a character
public class ActualGoapNode {
    public IPointOfInterest poiTarget { get; private set; }
    //public AlterEgoData poiTargetAlterEgo { get; private set; } //The alter ego the target was using while doing this action. only occupied if target is a character
    public Character actor { get; private set; }
    //public AlterEgoData actorAlterEgo { get; private set; } //The alter ego the character was using while doing this action.
    public bool isStealth { get; private set; }
    public object[] otherData { get; private set; }
    public int cost { get; private set; }

    public GoapAction action { get; private set; }
    public ACTION_STATUS actionStatus { get; private set; }
    public Log thoughtBubbleLog { get; private set; } //used if the current state of this action has a duration
    public Log thoughtBubbleMovingLog { get; private set; } //used when the actor is moving with this as his/her current action
    public Log descriptionLog { get; private set; } //action log at the end of the action
    public LocationStructure targetStructure { get; private set; }
    public LocationGridTile targetTile { get; private set; }
    public IPointOfInterest targetPOIToGoTo { get; private set; }
    public JOB_TYPE associatedJobType { get; private set; }

    public string currentStateName { get; private set; }
    public int currentStateDuration { get; private set; }
    
    public List<Character> awareCharacters { get; private set; }

    //public CRIME_TYPE crimeType { get; private set; }

    #region getters
    //TODO: Refactor these getters after all errors are resolved.
    public GoapActionState currentState {
        get {
            if (!action.states.ContainsKey(currentStateName)) {
                throw new System.Exception($"{action.goapName} does not have a state named {currentStateName}");
            }
            return action.states[currentStateName]; 
        }
    }
    public bool isPerformingActualAction {
        get { return actionStatus == ACTION_STATUS.PERFORMING; }
    }
    public bool isDone {
        get { return actionStatus == ACTION_STATUS.SUCCESS || actionStatus == ACTION_STATUS.FAIL; }
    }
    public INTERACTION_TYPE goapType {
        get { return action.goapType; }
    }
    public string goapName {
        get { return action.goapName; }
    }
    #endregion

    public ActualGoapNode(GoapAction action, Character actor, IPointOfInterest poiTarget, object[] otherData, int cost) {
        this.action = action;
        this.actor = actor;
        this.poiTarget = poiTarget;
        this.otherData = otherData;
        this.cost = cost;
        actionStatus = ACTION_STATUS.NONE;
        currentStateName = string.Empty;
        associatedJobType = JOB_TYPE.NONE;
        awareCharacters = new List<Character>();
        //Messenger.AddListener<string, ActualGoapNode>(Signals.ACTION_STATE_SET, OnActionStateSet);
    }

    //public void DestroyNode() {
    //    Messenger.RemoveListener<string, ActualGoapNode>(Signals.ACTION_STATE_SET, OnActionStateSet);
    //}

    #region Action
    public virtual void DoAction(JobQueueItem job, GoapPlan plan) {
        actionStatus = ACTION_STATUS.STARTED;
        associatedJobType = job.jobType;
        //Temporary only, create a system for this
        if(action.goapType == INTERACTION_TYPE.STEAL) {
            isStealth = true;
        }
        actor.SetCurrentActionNode(this, job, plan);
        CreateThoughtBubbleLog(targetStructure);
        //parentPlan?.SetPlanState(GOAP_PLAN_STATE.IN_PROGRESS);
        Messenger.Broadcast(Signals.CHARACTER_DOING_ACTION, actor, this);
        actor.marker.UpdateActionIcon();
        action.OnActionStarted(this);
        //poiTarget.AddTargettedByAction(this);

        //Move To Do Action
        actor.marker.pathfindingAI.ResetEndReachedDistance();
        SetTargetToGoTo();
        CheckAndMoveToDoAction(job);
    }
    private void SetTargetToGoTo() {
        if (targetStructure == null) {
            targetStructure = action.GetTargetStructure(this);
            if (targetStructure == null) { throw new System.Exception(
                $"{actor.name} target structure of action {action.goapName} is null."); }
        }
        if (action.actionLocationType == ACTION_LOCATION_TYPE.NEAR_TARGET || action.actionLocationType == ACTION_LOCATION_TYPE.NEAR_OTHER_TARGET) {
            IPointOfInterest targetToGoTo = action.GetTargetToGoTo(this);
            if (targetToGoTo == null) {
                targetTile = action.GetTargetTileToGoTo(this);
            } else {
                targetPOIToGoTo = targetToGoTo;
                targetTile = targetToGoTo.gridTileLocation;
            }
        } else if (action.actionLocationType == ACTION_LOCATION_TYPE.IN_PLACE) {
            targetTile = actor.gridTileLocation;
        } else if (action.actionLocationType == ACTION_LOCATION_TYPE.NEARBY) {
            if (actor.canMove) {
                List<LocationGridTile> choices = actor.gridTileLocation.GetTilesInRadius(3);
                if (choices.Count > 0) {
                    targetTile = choices[UtilityScripts.Utilities.rng.Next(0, choices.Count)];
                } else {
                    targetTile = actor.gridTileLocation;
                }
            } else {
                targetTile = actor.gridTileLocation;
            }
        } else if (action.actionLocationType == ACTION_LOCATION_TYPE.RANDOM_LOCATION) {
            List<LocationGridTile> choices = targetStructure.unoccupiedTiles.ToList();
            if (choices.Count > 0) {
                targetTile = choices[UtilityScripts.Utilities.rng.Next(0, choices.Count)];
            } else {
                throw new System.Exception(
                    $"{actor.name} target tile of action {action.goapName} for {action.actionLocationType} is null.");
            }
        } else if (action.actionLocationType == ACTION_LOCATION_TYPE.RANDOM_LOCATION_B) {
            targetTile = action.GetTargetTileToGoTo(this);
            if(targetTile == null) {
                List<LocationGridTile> choices = targetStructure.unoccupiedTiles.Where(x => x.UnoccupiedNeighbours.Count > 0).ToList();
                if (choices.Count > 0) {
                    targetTile = choices[UtilityScripts.Utilities.rng.Next(0, choices.Count)];
                } else {
                    throw new System.Exception(
                        $"{actor.name} target tile of action {action.goapName} for {action.actionLocationType} is null.");
                }
            }
        } else if (action.actionLocationType == ACTION_LOCATION_TYPE.RANDOM_LOCATION_B) {
            List<LocationGridTile> choices = targetStructure.unoccupiedTiles.Where(x => x.UnoccupiedNeighbours.Count > 0).ToList();
            if (choices.Count > 0) {
                targetTile = choices[UtilityScripts.Utilities.rng.Next(0, choices.Count)];
            } else {
                throw new System.Exception(
                    $"{actor.name} target tile of action {action.goapName} for {action.actionLocationType} is null.");
            }
        } else if (action.actionLocationType == ACTION_LOCATION_TYPE.TARGET_IN_VISION) {
            if (actor.marker.inVisionPOIs.Contains(poiTarget)) {
                targetTile = actor.gridTileLocation;
            } else {
                //No OnArriveAtTargetLocation because it doesn't trigger on arrival, rather, it is triggered by on vision
                targetPOIToGoTo = poiTarget;
                targetTile = poiTarget.gridTileLocation;
            }
        } else if (action.actionLocationType == ACTION_LOCATION_TYPE.OVERRIDE) {
            LocationGridTile tile = action.GetOverrideTargetTile(this);
            if (tile != null) {
                targetTile = tile;
            } else {
                throw new System.Exception(
                    $"{actor.name} override target tile of action {action.goapName} for {action.actionLocationType} is null.");
            }

        }
    }
    private void CheckAndMoveToDoAction(JobQueueItem job) {
        if(job.originalOwner == null) {
            //If somehow job is no longer available or is destroyed when trying to move to do action, do not continue
            //This happens when job is cancelled while actor is travelling to another region
            return;
        }
        if (!MoveToDoAction(job)) {
            if (targetTile != null) {
                //If cannot move to do action because there is no path between two location grid tiles, handle it here
                if (actor is Summon) {
                    (actor as Summon).NoPathToDoJob(job);
                } else if (actor.minion != null) {
                    actor.minion.NoPathToDoJob(job);
                }
                if (job.originalOwner.ownerType != JOB_OWNER.CHARACTER) {
                    job.AddBlacklistedCharacter(actor);
                }
                job.CancelJob(false);
            }
        }
    }
    //We only pass the job because we need to cancel it if the target tile is null
    private bool MoveToDoAction(JobQueueItem job) {
        //Only create thought bubble log when characters starts the action/moves to do the action so we can pass the target structure
        if (!actor.currentRegion.IsSameCoreLocationAs(targetTile.structure.location)) { //different core locations
            actor.currentParty.GoToLocation(targetTile.structure.location, PATHFINDING_MODE.NORMAL, doneAction: () => CheckAndMoveToDoAction(job));
        } else {
            if (targetTile == null) {
                //Here we check if there is a target tile to go to because if there is not, the target might already be destroyed/taken/disabled, if that happens, we must cancel job
                Debug.LogWarning(
                    $"{GameManager.Instance.TodayLogString()}{actor.name} is trying to move to do action {action.goapName} with target {poiTarget.name} but target tile is null, will cancel job {job.name} instead.");
                job.CancelJob(false);
                return false;
            }
            // LocationGridTile tileToGoTo = targetTile;
            // if (targetPOIToGoTo != null) {
            //     tileToGoTo = targetPOIToGoTo.gridTileLocation;
            // }
            // if (tileToGoTo == actor.gridTileLocation) {
            //     actor.marker.StopMovement();
            //     actor.PerformGoapAction();
            // } else {
            //     if (!PathfindingManager.Instance.HasPath(actor.gridTileLocation, tileToGoTo)) {
            //         return false;
            //     }
            //     actor.marker.GoTo(tileToGoTo, OnArriveAtTargetLocation);
            // }
            if (targetPOIToGoTo == null) {
                if (targetTile == actor.gridTileLocation) {
                    actor.marker.StopMovement();
                    actor.PerformGoapAction();
                } else {
                    if (!PathfindingManager.Instance.HasPath(actor.gridTileLocation, targetTile)) {
                        return false;
                    }
                    actor.marker.GoTo(targetTile, OnArriveAtTargetLocation);
                }
            } else {
                if(actor.gridTileLocation == targetPOIToGoTo.gridTileLocation) {
                    actor.marker.StopMovement();
                    actor.PerformGoapAction();
                } else {
                    if (!PathfindingManager.Instance.HasPath(actor.gridTileLocation, targetPOIToGoTo.gridTileLocation)) {
                        return false;
                    }
                    actor.marker.GoToPOI(targetPOIToGoTo, OnArriveAtTargetLocation);
                }
            }
        }
        return true;
    }
    //private void MoveToDoAction() {
    //    if (targetStructure == null) {
    //        targetStructure = action.GetTargetStructure(this);
    //        if (targetStructure == null) { throw new System.Exception(actor.name + " target structure of action " + action.goapName + " is null."); }
    //    }
    //    //Only create thought bubble log when characters starts the action/moves to do the action so we can pass the target structure
    //    CreateThoughtBubbleLog(targetStructure);
    //    if (!actor.currentRegion.IsSameCoreLocationAs(targetStructure.location)) { //different core locations
    //        actor.currentParty.GoToLocation(targetStructure.location, PATHFINDING_MODE.NORMAL, doneAction: MoveToDoAction);
    //    } else {
    //        if (action.actionLocationType == ACTION_LOCATION_TYPE.NEAR_TARGET || action.actionLocationType == ACTION_LOCATION_TYPE.NEAR_OTHER_TARGET) {
    //            IPointOfInterest targetToGoTo = action.GetTargetToGoTo(this);
    //            if(targetToGoTo == null) {
    //                targetTile = action.GetTargetTileToGoTo(this);
    //                actor.marker.GoTo(targetTile, OnArriveAtTargetLocation);
    //            } else {
    //                targetTile = targetToGoTo.gridTileLocation;
    //                actor.marker.GoTo(targetToGoTo, OnArriveAtTargetLocation);
    //            }
    //        } else if (action.actionLocationType == ACTION_LOCATION_TYPE.IN_PLACE) {
    //            targetTile = actor.gridTileLocation;
    //            actor.PerformGoapAction();
    //        } else if (action.actionLocationType == ACTION_LOCATION_TYPE.NEARBY) {
    //            List<LocationGridTile> choices = actor.currentNpcSettlement.areaMap.GetTilesInRadius(actor.gridTileLocation, 3);
    //            if (choices.Count > 0) {
    //                targetTile = choices[Utilities.rng.Next(0, choices.Count)];
    //                actor.marker.GoTo(targetTile, OnArriveAtTargetLocation);
    //            } else {
    //                targetTile = actor.gridTileLocation;
    //                actor.PerformGoapAction();
    //            }
    //        } else if (action.actionLocationType == ACTION_LOCATION_TYPE.RANDOM_LOCATION) {
    //            List<LocationGridTile> choices = targetStructure.unoccupiedTiles;
    //            if (choices.Count > 0) {
    //                targetTile = choices[Utilities.rng.Next(0, choices.Count)];
    //                actor.marker.GoTo(targetTile, OnArriveAtTargetLocation);
    //            } else {
    //                throw new System.Exception(actor.name + " target tile of action " + action.goapName + " for " + action.actionLocationType.ToString() + " is null.");
    //            }
    //        } else if (action.actionLocationType == ACTION_LOCATION_TYPE.RANDOM_LOCATION_B) {
    //            List<LocationGridTile> choices = targetStructure.unoccupiedTiles.Where(x => x.UnoccupiedNeighbours.Count > 0).ToList();
    //            if (choices.Count > 0) {
    //                targetTile = choices[Utilities.rng.Next(0, choices.Count)];
    //                actor.marker.GoTo(targetTile, OnArriveAtTargetLocation);
    //            } else {
    //                throw new System.Exception(actor.name + " target tile of action " + action.goapName + " for " + action.actionLocationType.ToString() + " is null.");
    //            }
    //        } else if (action.actionLocationType == ACTION_LOCATION_TYPE.RANDOM_LOCATION_B) {
    //            List<LocationGridTile> choices = targetStructure.unoccupiedTiles.Where(x => x.UnoccupiedNeighbours.Count > 0).ToList();
    //            if (choices.Count > 0) {
    //                targetTile = choices[Utilities.rng.Next(0, choices.Count)];
    //                actor.marker.GoTo(targetTile, OnArriveAtTargetLocation);
    //            } else {
    //                throw new System.Exception(actor.name + " target tile of action " + action.goapName + " for " + action.actionLocationType.ToString() + " is null.");
    //            }
    //        } else if (action.actionLocationType == ACTION_LOCATION_TYPE.TARGET_IN_VISION) {
    //            if (actor.marker.inVisionPOIs.Contains(poiTarget)) {
    //                targetTile = actor.gridTileLocation;
    //                actor.PerformGoapAction();
    //            } else {
    //                //No OnArriveAtTargetLocation because it doesn't trigger on arrival, rather, it is triggered by on vision
    //                targetTile = poiTarget.gridTileLocation;
    //                actor.marker.GoTo(poiTarget);
    //            }
    //        } else if (action.actionLocationType == ACTION_LOCATION_TYPE.OVERRIDE) {
    //            LocationGridTile tile = action.GetOverrideTargetTile(this);
    //            if (tile != null) {
    //                targetTile = tile;
    //                actor.marker.GoTo(tile, OnArriveAtTargetLocation);
    //            } else {
    //                throw new System.Exception(actor.name + " override target tile of action " + action.goapName + " for " + action.actionLocationType.ToString() + " is null.");
    //            }
                
    //        }
    //    }
    //}
    private void OnArriveAtTargetLocation() {
        if(action.actionLocationType != ACTION_LOCATION_TYPE.TARGET_IN_VISION) {
            //Only do perform goap action on arrive at location if the location type is not target in vision, because if it is, we no longer need this function because perform goap action is already called upon entering vision
            actor.PerformGoapAction();
        }
    }
    public void PerformAction() {
        GoapActionInvalidity goapActionInvalidity = action.IsInvalid(this);
        if (goapActionInvalidity.isInvalid) {
            Debug.Log($"{GameManager.Instance.TodayLogString()}{actor.name}'s action {action.goapType.ToString()} was invalid!");
            action.LogActionInvalid(goapActionInvalidity, this);
            actor.GoapActionResult(InteractionManager.Goap_State_Fail, this);
            action.OnInvalidAction(this);
            return;
        }
        actionStatus = ACTION_STATUS.PERFORMING;
        actor.marker.UpdateAnimation();

        if (poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            Character targetCharacter = poiTarget as Character;
            if (!action.doesNotStopTargetCharacter && actor != poiTarget) {
                if (!targetCharacter.isDead) {
                    if (targetCharacter.currentParty.icon.isTravelling) {
                        if (targetCharacter.currentParty.icon.travelLine == null) {
                            //This means that the actor currently travelling to another tile in tilemap
                            targetCharacter.marker.StopMovement();
                        }
                    }
                    if (targetCharacter.currentActionNode != null) {
                        targetCharacter.StopCurrentActionNode(false);
                    }
                    targetCharacter.DecreaseCanMove();
                    InnerMapManager.Instance.FaceTarget(targetCharacter, actor);
                }
                targetCharacter.AdjustIsStoppedByOtherCharacter(1);
            }
        }
        if (action.actionCategory != ACTION_CATEGORY.INDIRECT && poiTarget is BaseMapObject baseMapObject) {
            baseMapObject.OnManipulatedBy(actor);
        }
        action.Perform(this);
        Messenger.Broadcast(Signals.ACTION_PERFORMED, this);
        //CRIME_TYPE crimeType = CrimeManager.Instance.GetCrimeType(this);
        //if(crimeType != CRIME_TYPE.NONE) {
        //    CrimeManager.Instance.MakeCharacterACriminal(actor, crimeType, action);
        //}
    }
    public void ActionInterruptedWhilePerforming() {
        string log =
            $"{GameManager.Instance.TodayLogString()}{actor.name} is interrupted while doing goap action: {action.goapName}";
        string result = GoapActionStateDB.GetStateResult(action.goapType, currentState.name);
        if (result == InteractionManager.Goap_State_Success) {
            actionStatus = ACTION_STATUS.SUCCESS;
        } else {
            actionStatus = ACTION_STATUS.FAIL;
        }
        StopPerTickEffect();
        if (poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER && !action.doesNotStopTargetCharacter && actor != poiTarget) {
            Character targetCharacter = poiTarget as Character;
            if (!targetCharacter.isDead) {
                targetCharacter.IncreaseCanMove();
            }
            targetCharacter.AdjustIsStoppedByOtherCharacter(-1);
        }
        OnFinishActionTowardsTarget();
        GoapPlanJob job = actor.currentJob as GoapPlanJob;
        if (actor.currentActionNode == this) {
            actor.SetCurrentActionNode(null, null, null);
        }
        //Assert.IsNotNull(job, $"{actor.name} was interrupted when performing {action.goapName} but, in this process his/her current job is null!");
        job?.CancelJob(false);
        Messenger.Broadcast(Signals.CHARACTER_FINISHED_ACTION, this);
    }
    public void ActionResult(GoapActionState actionState) {
        string result = GoapActionStateDB.GetStateResult(action.goapType, actionState.name);
        if (result == InteractionManager.Goap_State_Success) {
            actionStatus = ACTION_STATUS.SUCCESS;
        } else {
            actionStatus = ACTION_STATUS.FAIL;
        }
        //actor.OnCharacterDoAction(this);
        StopPerTickEffect();
        //endedAtState = currentState;
        //actor.PrintLogIfActive(action.goapType.ToString() + " action by " + this.actor.name + " Summary: \n" + actionSummary);
        if (poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER && !action.doesNotStopTargetCharacter && actor != poiTarget) {
            Character targetCharacter = poiTarget as Character;
            if (!targetCharacter.isDead) {
                //if (resumeTargetCharacterState) {
                //    if (targetCharacter.stateComponent.currentState != null && targetCharacter.stateComponent.currentState.isPaused) {
                //        targetCharacter.stateComponent.currentState.ResumeState();
                //    }
                //}
                targetCharacter.IncreaseCanMove();
            }
            targetCharacter.AdjustIsStoppedByOtherCharacter(-1);
        }
        //else {
        //    Messenger.RemoveListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        //    Messenger.RemoveListener<TileObject, Character>(Signals.TILE_OBJECT_DISABLED, OnTileObjectDisabled);
        //}
        OnFinishActionTowardsTarget();
        actor.GoapActionResult(result, this);
        //if (endAction != null) {
        //    endAction(result, this);
        //} else {
        //    if (parentPlan != null) {
        //        //Do not go to result if there is no parent plan, this might mean that the action is just a forced action
        //        actor.GoapActionResult(result, this);
        //    }
        //}
        Messenger.Broadcast(Signals.CHARACTER_FINISHED_ACTION, this);
        //parentPlan?.OnActionInPlanFinished(actor, this, result);
    }
    public void StopActionNode(bool shouldDoAfterEffect) {
        if (actionStatus == ACTION_STATUS.PERFORMING) {
            action.OnStopWhilePerforming(this);
            if (currentState.duration == 0) { //If action has no duration then do EndPerTickEffect (this will also call the action result)
                OnCancelActionTowardsTarget();
                //ReturnToActorTheActionResult(InteractionManager.Goap_State_Fail);
                EndPerTickEffect(shouldDoAfterEffect);
            } else { //If action has duration and interrupted in the middle of the duration then do ActionInterruptedWhilePerforming (this will not call the action result, instead it will call the cancel job so it can be brought back to the npcSettlement list if it is a npcSettlement job)
                ActionInterruptedWhilePerforming();
            }
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
    #endregion

    #region Action State
    public void OnActionStateSet(string stateName) {
        Debug.Log($"Set action state of {actor.name}'s {action.goapName} to {stateName}");
        currentStateName = stateName;
        OnPerformActualActionToTarget();
        ExecuteCurrentActionState();
    }
    private void ExecuteCurrentActionState() {
        if (!action.states.ContainsKey(currentStateName)) {
            Debug.LogError(
                $"Failed to execute current action state for {actor.name} because {action.goapName} does not have state with name: {currentStateName}");
        }
        Debug.Log($"Executing action state of {actor.name}'s {action.goapName}, {currentStateName}");
        GoapActionState currentState = action.states[currentStateName];

        IPointOfInterest target = poiTarget;
        if(poiTarget is TileObject && action.goapType == INTERACTION_TYPE.STEAL) {
            TileObject item = poiTarget as TileObject;
            if(item.isBeingCarriedBy != null) {
                target = item.isBeingCarriedBy;
            }
        }

        if (isStealth && target.traitContainer.HasTrait("Vigilant")) {
            descriptionLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "vigilant", this);
            action.AddFillersToLog(descriptionLog, this);
            descriptionLog.AddToFillers(null, action.name, LOG_IDENTIFIER.STRING_1);
        } else {
            CreateDescriptionLog(currentState);
            currentState.preEffect?.Invoke(this);
            for (int i = 0; i < actor.traitContainer.allTraitsAndStatuses.Count; i++) {
                Trait currTrait = actor.traitContainer.allTraitsAndStatuses[i];
                currTrait.ExecuteActionPreEffects(action.goapType, this);
            }
            for (int i = 0; i < poiTarget.traitContainer.allTraitsAndStatuses.Count; i++) {
                Trait currTrait = poiTarget.traitContainer.allTraitsAndStatuses[i];
                currTrait.ExecuteActionPreEffects(action.goapType, this);
            }
        }

        actor.marker.UpdateAnimation();
        //parentAction.SetExecutionDate(GameManager.Instance.Today());

        if (currentState.duration > 0) {
            currentStateDuration = 0;
            StartPerTickEffect();
        } else if (currentState.duration != -1) {
            EndPerTickEffect();
        }
    }
    private void StartPerTickEffect() {
        Messenger.AddListener(Signals.TICK_STARTED, PerTickEffect);
    }
    public void StopPerTickEffect() {
        Messenger.RemoveListener(Signals.TICK_STARTED, PerTickEffect);
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
        if (shouldDoAfterEffect) {
            if (descriptionLog != null && action.shouldAddLogs) { //only add logs if both the parent action and this state should add logs
                //if (descriptionLog != null) {
                //    AddArrangedLog("description", descriptionLog, null);
                //}
                if (!action.isNotificationAnIntel) {
                    // PlayerManager.Instance.player.ShowNotificationFrom(descriptionLog, actor);
                    // Messenger.Broadcast(Signals.SHOW_PLAYER_NOTIFICATION, descriptionLog);
                } else {
                    PlayerManager.Instance.player.ShowNotificationFrom(actor, InteractionManager.Instance.CreateNewIntel(this));
                    // Messenger.Broadcast(Signals.SHOW_INTEL_NOTIFICATION, InteractionManager.Instance.CreateNewIntel(this));
                }
                descriptionLog.AddLogToInvolvedObjects();
                //for (int i = 0; i < arrangedLogs.Count; i++) {
                //    arrangedLogs[i].log.SetDate(GameManager.Instance.Today());
                //    arrangedLogs[i].log.AddLogToInvolvedObjects();
                //}
            }
        }
        GoapActionState currentState = action.states[currentStateName];
        ActionResult(currentState);

        IPointOfInterest target = poiTarget;
        if(poiTarget is TileObject && action.goapType == INTERACTION_TYPE.STEAL) {
            TileObject item = poiTarget as TileObject;
            if(item.isBeingCarriedBy != null) {
                target = item.isBeingCarriedBy;
            }
        }

        //After effect and logs should be done after processing action result so that we can be sure that the action is completely done before doing anything
        if (shouldDoAfterEffect && !(isStealth && target.traitContainer.HasTrait("Vigilant"))) {
            currentState.afterEffect?.Invoke(this);
            bool isRemoved = false;
            for (int i = 0; i < actor.traitContainer.statuses.Count; i++) {
                Trait currTrait = actor.traitContainer.statuses[i];
                isRemoved = false;
                currTrait.ExecuteActionAfterEffects(action.goapType, this, ref isRemoved);
                if (isRemoved) { i--; }
            }
            for (int i = 0; i < poiTarget.traitContainer.statuses.Count; i++) {
                Trait currTrait = poiTarget.traitContainer.statuses[i];
                isRemoved = false;
                currTrait.ExecuteActionAfterEffects(action.goapType, this, ref isRemoved);
                if (isRemoved) { i--; }
            }
        }
        //else {
        //    parentAction.SetShowIntelNotification(false);
        //}
        //actor.OnCharacterDoAction(parentAction); //Moved this here to fix intel not being shown, because arranged logs are not added until after the ReturnToActorTheActionResult() call.
        //if (shouldDoAfterEffect) {
        //    action.AfterAfterEffect();
        //}
    }
    private void PerTickEffect() {
        GoapActionState currentState = action.states[currentStateName];
        currentStateDuration++;

        IPointOfInterest target = poiTarget;
        if(poiTarget is TileObject && action.goapType == INTERACTION_TYPE.STEAL) {
            TileObject item = poiTarget as TileObject;
            if(item.isBeingCarriedBy != null) {
                target = item.isBeingCarriedBy;
            }
        }
        if (!(isStealth && target.traitContainer.HasTrait("Vigilant"))) {
            currentState.perTickEffect?.Invoke(this);
            for (int i = 0; i < actor.traitContainer.allTraitsAndStatuses.Count; i++) {
                Trait currTrait = actor.traitContainer.allTraitsAndStatuses[i];
                currTrait.ExecuteActionPerTickEffects(action.goapType, this);
            }
            for (int i = 0; i < poiTarget.traitContainer.allTraitsAndStatuses.Count; i++) {
                Trait currTrait = poiTarget.traitContainer.allTraitsAndStatuses[i];
                currTrait.ExecuteActionPerTickEffects(action.goapType, this);
            }
        }
        if (currentStateDuration >= currentState.duration) {
            EndPerTickEffect();
        }
    }
    private void OnPerformActualActionToTarget() {
        if (GoapActionStateDB.GetStateResult(action.goapType, currentStateName) != InteractionManager.Goap_State_Success) {
            return;
        }
        if (poiTarget is TileObject target) {
            target.OnDoActionToObject(this);
        }
        //else if (poiTarget is Character) {
        //    if (currentState.name != "Target Missing" && !doesNotStopTargetCharacter) {
        //        AddAwareCharacter(poiTarget as Character);
        //    }
        //}
    }
    private void OnFinishActionTowardsTarget() {
        if (actionStatus == ACTION_STATUS.FAIL) {
            return;
        }
        if (poiTarget is TileObject) {
            TileObject target = poiTarget as TileObject;
            target.OnDoneActionToObject(this);
        }
    }
    private void OnCancelActionTowardsTarget() {
        if (poiTarget is TileObject) {
            TileObject target = poiTarget as TileObject;
            target.OnCancelActionTowardsObject(this);
        }
    }
    public void OverrideCurrentStateDuration(int val) {
        currentStateDuration = val;
    }
    #endregion

    #region Log
    private void CreateDescriptionLog(GoapActionState actionState) {
        if (descriptionLog == null) {
            descriptionLog = actionState.CreateDescriptionLog(actor, poiTarget, this);
        }
    }
    private void CreateThoughtBubbleLog(LocationStructure targetStructure) {
        if(thoughtBubbleLog == null) {
            if (LocalizationManager.Instance.HasLocalizedValue("GoapAction", action.goapName, "thought_bubble")) {
                thoughtBubbleLog = new Log(GameManager.Instance.Today(), "GoapAction", action.goapName, "thought_bubble", this);
                action.AddFillersToLog(thoughtBubbleLog, this);
            }
        }
        if (thoughtBubbleMovingLog == null) {
            if (LocalizationManager.Instance.HasLocalizedValue("GoapAction", action.goapName, "thought_bubble_m")) {
                thoughtBubbleMovingLog = new Log(GameManager.Instance.Today(), "GoapAction", action.goapName, "thought_bubble_m", this);
                action.AddFillersToLog(thoughtBubbleMovingLog, this);
            }
        }
    }
    public Log GetCurrentLog() {
        if(actionStatus == ACTION_STATUS.STARTED) {
            return thoughtBubbleMovingLog;
        }else if (actionStatus == ACTION_STATUS.PERFORMING) {
            return thoughtBubbleLog;
        }
        return descriptionLog;
        //if (onlyShowNotifOfDescriptionLog && currentState != null) {
        //    return this.currentState.descriptionLog;
        //}
        //if (actor.currentParty.icon.isTravelling) {
        //    if (currentState != null) {
        //        //character is travelling but there is already a current state
        //        //Note: this will only happen is action has whileMovingState
        //        //Examples are: Imprison Character and Abduct Character actions
        //        return currentState.descriptionLog;
        //    }
        //    //character is travelling
        //    return thoughtBubbleMovingLog;
        //} else {
        //    //character is not travelling
        //    if (this.isDone) {
        //        //action is already done
        //        return this.currentState.descriptionLog;
        //    } else {
        //        //action is not yet done
        //        if (currentState == null) {
        //            //if the actions' current state is null, Use moving log
        //            return thoughtBubbleMovingLog;
        //        } else {
        //            //if the actions current state has a duration
        //            return thoughtBubbleLog;
        //        }
        //    }
        //}
    }
    public void OverrideDescriptionLog(Log log) {
        descriptionLog = log;
    }
    public string StringText() {
        return $"{action.goapName} with actor => {actor.name}, and target => {poiTarget.name}";
    }
    #endregion

    #region Jobs
    public void OnAttachPlanToJob(GoapPlanJob job) {
        isStealth = job.isStealth;
    }
    #endregion
    
    #region Character
    public void AddAwareCharacter(Character character) {
        awareCharacters.Add(character);
    }
    #endregion

    public override string ToString() {
        return $"Action: {action?.name ?? "Null"}. Target {poiTarget?.name ?? "Null"}";
    }

    //#region Crime
    //public void SetAsCrime(CRIME_TYPE crimeType) {
    //    this.crimeType = crimeType;
    //}
    //#endregion
}