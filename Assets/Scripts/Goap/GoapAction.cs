﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Reflection;
using System.Linq;

public class GoapAction {

    public INTERACTION_TYPE goapType { get; private set; }
    public INTERACTION_ALIGNMENT alignment { get; private set; }
    public string goapName { get; protected set; }
    public IPointOfInterest poiTarget { get; private set; }
    public Character actor { get; private set; }
    public int cost { get { return GetCost() + GetDistanceCost(); } }
    public List<Precondition> preconditions { get; private set; }
    public List<GoapEffect> expectedEffects { get; private set; }
    public virtual LocationStructure targetStructure {
        get {
            try {
                if (poiTarget is TileObject || poiTarget is SpecialToken) {
                    //if the target is a tile object or a special token, the actor will always go to it's known location instead of actual
                    IAwareness awareness = actor.GetAwareness(poiTarget);
                    if (awareness != null) {
                        return awareness.knownGridLocation.structure;
                    }
                }
                return poiTarget.gridTileLocation.structure;
            } catch {
                throw new Exception("Error with target structure in " + goapName + " targetting " + poiTarget.ToString() + " by " + actor.name);
            }
            
        }
    }
    public LocationGridTile targetTile { get; protected set; }
    public Dictionary<string, GoapActionState> states { get; protected set; }
    public List<GoapEffect> actualEffects { get; private set; } //stores what really happened. NOTE: Only storing relevant data to share intel, no need to store everything that happened.
    public Log thoughtBubbleLog { get; protected set; }
    public GoapActionState currentState { get; private set; }
    public GoapPlan parentPlan { get { return actor.GetPlanWithAction(this); } }
    public bool isStopped { get; private set; }
    public bool isPerformingActualAction { get; private set; }
    public bool isDone { get; private set; }
    public ACTION_LOCATION_TYPE actionLocationType { get; protected set; } //This is set in every action's constructor
    public bool showIntelNotification { get; protected set; } //should this action show a notification when it is done by its actor or when it recieves a plan with this action as it's end node?
    public bool shouldAddLogs { get; protected set; } //should this action add logs to it's actor?
    public string actionIconString { get; protected set; }
    public GameDate executionDate { get; protected set; }
    protected virtual string failActionState { get { return "Target Missing"; } }

    protected Func<bool> _requirementAction;
    protected System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
    protected string actionSummary;
    protected TIME_IN_WORDS[] validTimeOfDays;

    public GoapAction(INTERACTION_TYPE goapType, INTERACTION_ALIGNMENT alignment, Character actor, IPointOfInterest poiTarget) {
        this.alignment = alignment;
        this.goapType = goapType;
        this.goapName = Utilities.NormalizeStringUpperCaseFirstLetters(goapType.ToString());
        this.poiTarget = poiTarget;
        this.actor = actor;
        isStopped = false;
        isPerformingActualAction = false;
        isDone = false;
        showIntelNotification = true;
        shouldAddLogs = true;
        preconditions = new List<Precondition>();
        expectedEffects = new List<GoapEffect>();
        actualEffects = new List<GoapEffect>();
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Joy_Icon;
        actionSummary = GameManager.Instance.TodayLogString() + actor.name + " created " + goapType.ToString() + " action, targetting " + poiTarget?.ToString() ?? "Nothing";
    }

    //public void SetParentPlan(GoapPlan plan) {
    //    parentPlan = plan;
    //}

    #region States
    public void SetState(string stateName) {
        currentState = states[stateName];
        AddActionLog(GameManager.Instance.TodayLogString() + " Set state to " + currentState.name);
        OnPerformActualActionToTarget();
        currentState.Execute();
    }
    #endregion

    #region Virtuals
    protected virtual void CreateStates() {
        string summary = "Creating states for goap action (Dynamic) " + goapType.ToString() + " for actor " + actor.name;
        sw.Start();
        states = new Dictionary<string, GoapActionState>();
        if (GoapActionStateDB.goapActionStates.ContainsKey(this.goapType)) {
            StateNameAndDuration[] statesSetup = GoapActionStateDB.goapActionStates[this.goapType];
            for (int i = 0; i < statesSetup.Length; i++) {
                StateNameAndDuration state = statesSetup[i];
                string trimmedState = Utilities.RemoveWhitespace(state.name);
                Type thisType = this.GetType();
                MethodInfo preMethod = thisType.GetMethod("Pre" + trimmedState, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                MethodInfo perMethod = thisType.GetMethod("PerTick" + trimmedState, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                MethodInfo afterMethod = thisType.GetMethod("After" + trimmedState, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                Action preAction = null;
                Action perAction = null;
                Action afterAction = null;
                if (preMethod != null) {
                    preAction = (Action)Delegate.CreateDelegate(typeof(Action), this, preMethod, false);
                }
                if (perMethod != null) {
                    perAction = (Action)Delegate.CreateDelegate(typeof(Action), this, perMethod, false);
                }
                if (afterMethod != null) {
                    afterAction = (Action)Delegate.CreateDelegate(typeof(Action), this, afterMethod, false);
                }
                GoapActionState newState = new GoapActionState(state.name, this, preAction, perAction, afterAction, state.duration, state.status);
                states.Add(state.name, newState);
                summary += "\n Creating state " + state.name;
            }

        }
        sw.Stop();
        //Debug.Log(summary + "\n" + string.Format("Total creation time is {0}ms", sw.ElapsedMilliseconds));

    }
    protected virtual void ConstructPreconditionsAndEffects() { }
    protected virtual void ConstructRequirement() { }
    protected virtual int GetCost() {
        return 0;
    }
    public virtual void PerformActualAction() {
        isPerformingActualAction = true;
        if (poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            Character targetCharacter = poiTarget as Character;
            if (targetCharacter != actor) {
                targetCharacter.FaceTarget(actor);
            }
        }
    }
    protected virtual void CreateThoughtBubbleLog() {
        thoughtBubbleLog = new Log(GameManager.Instance.Today(), "GoapAction", this.GetType().ToString(), "thought_bubble");
        if (thoughtBubbleLog != null) {
            thoughtBubbleLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            thoughtBubbleLog.AddToFillers(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER); //Target character is only the identifier but it doesn't mean that this is a character, it can be item, etc.
            if (targetStructure != null) {
                thoughtBubbleLog.AddToFillers(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
            } else {
                thoughtBubbleLog.AddToFillers(actor.specificLocation, actor.specificLocation.name, LOG_IDENTIFIER.LANDMARK_1);
            }
        }
    }

    ///<summary>
    ///This is called when the actor decides to do this specific action.
    ///All movement related actions should be done here.
    ///</summary>
    ///<param name="plan">Plan where this action came from.</param>
    public virtual void DoAction(GoapPlan plan) {
        CreateStates(); //Not sure if this is the best place for this.
        actor.SetCurrentAction(this);
        plan.SetPlanState(GOAP_PLAN_STATE.IN_PROGRESS);
        Messenger.Broadcast(Signals.CHARACTER_DOING_ACTION, actor, this);

        //if the current target is a character, make him/her wait for this action
        Character targetCharacter = null;
        if (poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            if((poiTarget as Character) != actor) {
                targetCharacter = poiTarget as Character;
                string log = actor.name + " is planning to do something to " + targetCharacter.name + " at " + actor.specificLocation.name;
                //GameManager.Instance.SetPausedState(true);
                if (targetCharacter.currentParty.icon.isTravelling && targetCharacter.currentParty.icon.travelLine == null) {
                    targetCharacter.marker.StopMovement(() => MoveToDoAction(plan, targetCharacter));
                    log += "\n- " + targetCharacter.name + " is currently travelling, stopping movement";
                    Debug.LogWarning(log);
                    return;
                } else {
                    log += "\n- " + targetCharacter.name + " is not travelling, actor is moving towards it...";
                    Debug.LogWarning(log);
                }

            }
        }

        //if the specified target tile is null. Use the default means to get the target tile. (Uses Action Location Type)
        //if (targetTile == null) {
        //    targetTile = GetTargetLocationTile();
        //}
        //if(this.targetTile == null) {
        //    this.targetTile = targetTile;
        //}
        MoveToDoAction(plan, targetCharacter);
    }
    public virtual LocationGridTile GetTargetLocationTile() {
        LocationGridTile knownTargetLocation = null;
        if (poiTarget is TileObject || poiTarget is SpecialToken) {
            //if the target is a tile object or a special token, the actor will always go to it's known location instead of actual
            knownTargetLocation = actor.GetAwareness(poiTarget).knownGridLocation;
        } else {
            knownTargetLocation = poiTarget.gridTileLocation;
        }
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, knownTargetLocation, targetStructure);
    }
    public virtual void SetTargetStructure() {
        if(targetStructure != null) {
            targetTile = GetTargetLocationTile();
        }
    }
    //This is for the waiting time, if this returns true, this action will not be done by the actor momentarily, this will be skipped until this returns false
    public virtual bool IsHalted() {
        //Only waiting condition for now is the time of day
        //The default for the valid time of days is null, if it is null, do not wait meaning return false
        //if (validTimeOfDays != null && !validTimeOfDays.Contains(GameManager.GetCurrentTimeInWordsOfTick())) {
        //    return true;
        //}
        return false;
    }
    public virtual void ExecuteTargetMissing() {
        if (states.ContainsKey("Target Missing")) {
            SetState("Target Missing");
        } else {
            //for cases that the current action has no target missing state
            FailAction();
        }
    }
    #endregion

    #region Utilities
    public void Initialize() {
        ConstructRequirement();
        ConstructPreconditionsAndEffects();
        SetTargetStructure();
        CreateThoughtBubbleLog();
    }
    public bool IsThisPartOfActorActionPool(Character actor) {
        List<INTERACTION_TYPE> actorInteractions = RaceManager.Instance.GetNPCInteractionsOfRace(actor);
        return actorInteractions.Contains(goapType);
    }
    public bool CanSatisfyRequirements() {
        bool requirementActionSatisfied = true;
        if(_requirementAction != null) {
            requirementActionSatisfied = _requirementAction();
        }
        return requirementActionSatisfied && (validTimeOfDays == null || validTimeOfDays.Contains(GameManager.GetCurrentTimeInWordsOfTick()));
    }
    public void AddTraitTo(Character target, string traitName) {
        if (target.AddTrait(traitName)) {
            AddActualEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.ADD_TRAIT, conditionKey = traitName, targetPOI = target });
        }
    }
    public void RemoveTraitFrom(Character target, string traitName) {
        if (target.RemoveTrait(traitName)) {
            AddActualEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = traitName, targetPOI = target });
        }
    }
    public void ReturnToActorTheActionResult(string result) {
        actor.OnCharacterDoAction(this);
        currentState.StopPerTickEffect();
        End();
        actor.GoapActionResult(result, this);
        Messenger.Broadcast(Signals.CHARACTER_FINISHED_ACTION, actor, this, result);
    }
    protected void AddActionLog(string log) {
        actionSummary += "\n" + log;
    }
    public void End() {
        isPerformingActualAction = false;
        isDone = true;
        if (Messenger.eventTable.ContainsKey(Signals.CHARACTER_DEATH)) {
            Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnActorDied);
        }
        this.actor.PrintLogIfActive(this.goapType.ToString() + " action by " + this.actor.name + " Summary: \n" + actionSummary);
    }
    public void StopAction( ) {
        //GoapAction action = actor.currentAction;
        if(actor.marker.pathfindingThread != null) {
            actor.marker.pathfindingThread.SetDoNotMove(true);
        }
        actor.SetCurrentAction(null);
        if (actor.currentParty.icon.isTravelling && actor.currentParty.icon.travelLine == null) {
            //This means that the actor currently travelling to another tile in tilemap
            actor.marker.StopMovement();
        }
        SetIsStopped(true);
        if(isPerformingActualAction && !isDone) {
            ReturnToActorTheActionResult(InteractionManager.Goap_State_Fail);
        } else {
            //if (action != null && action.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            //    Character targetCharacter = action.poiTarget as Character;
            //    targetCharacter.AdjustIsWaitingForInteraction(-1);
            //}
            if (!actor.DropPlan(parentPlan)) {
                actor.PlanGoapActions();
            }
        }
        if (UIManager.Instance.characterInfoUI.isShowing) {
            UIManager.Instance.characterInfoUI.UpdateBasicInfo();
        }
        //Messenger.Broadcast<GoapAction>(Signals.STOP_ACTION, this);
    }
    public void SetIsStopped(bool state) {
        isStopped = state;
    }
    public int GetDistanceCost() {
        if (actor.specificLocation != targetStructure.location) {
            return 3;
        } else {
            LocationGridTile tile = targetTile;
            if (tile == null) {
                tile = poiTarget.gridTileLocation;
            }
            int distance = Mathf.RoundToInt(actor.gridTileLocation.GetDistanceTo(tile));
            return distance / 6;
            //try {
            //    int distance = Mathf.RoundToInt(actor.gridTileLocation.GetDistanceTo(tile));
            //    return distance / 6;
            //} catch(System.Exception e) {
            //    if(actor.gridTileLocation == null) {
            //        Console.WriteLine("ACTOR TILE LOCATION IS NULL!");
            //    } else if (tile == null) {
            //        Console.WriteLine("TILE IS NULL!");
            //    }
            //}
            //return 0;
        }
    }
    protected bool HasSupply(int neededSupply) {
        return actor.supply >= neededSupply;
    }
    /// <summary>
    /// This is used by the character marker so that when it recalculates a path, his/her current action is updated.
    /// </summary>
    /// <param name="targetTile">The new target tile.</param>
    public void UpdateTargetTile(LocationGridTile targetTile) {
        this.targetTile = targetTile;
    }
    public void SetExecutionDate(GameDate date) {
        executionDate = date;
    }
    private void MoveToDoAction(GoapPlan plan, Character targetCharacter) {
        if(targetCharacter != null) {
            targetCharacter.AdjustIsWaitingForInteraction(1);
            if (targetCharacter.currentAction != null && !targetCharacter.currentAction.isPerformingActualAction && !targetCharacter.currentAction.isDone) {
                targetCharacter.SetCurrentAction(null);
                //log += "\n- " + targetCharacter.name + " is not performing actual action setting current action to null...";
            }
        }
        //if the actor is NOT at the area where the target structure is, make him/her go there first.
        if (actor.specificLocation != targetStructure.location) {
            actor.currentParty.GoToLocation(targetStructure.location, PATHFINDING_MODE.NORMAL, targetStructure, () => actor.PerformGoapAction(plan), null, null, poiTarget, targetTile);
        } else {
            //if the actor is already at the area where the target structure is, just make the actor move to the specified target structure (ususally the structure where the poiTarget is at).
            actor.MoveToAnotherStructure(targetStructure, targetTile, poiTarget, () => actor.PerformGoapAction(plan));
            //actor.PerformGoapAction(plan);
        }
    }
    public void FailAction() {
        //if (goapType == INTERACTION_TYPE.SLEEP) {
        //    Debug.LogError(actor.name + " failed " + goapName + " action from recalculate path!");
        //}
        if (actor.currentParty.icon.isTravelling && actor.currentParty.icon.travelLine == null) {
            //This means that the actor currently travelling to another tile in tilemap
            actor.marker.StopMovement(() => SetState(failActionState));
        } else {
            SetState(failActionState);
        }
        //Set state to failed after this (in overrides)
    }
    #endregion

    #region Preconditions
    protected void AddPrecondition(GoapEffect effect, Func<bool> condition) {
        preconditions.Add(new Precondition(effect, condition));
    }
    public bool CanSatisfyAllPreconditions() {
        for (int i = 0; i < preconditions.Count; i++) {
            if (!preconditions[i].CanSatisfyCondition()) {
                return false;
            }
        }
        return true;
    }
    #endregion

    #region Effects
    protected void AddExpectedEffect(GoapEffect effect) {
        expectedEffects.Add(effect);
    }
    public bool WillEffectsSatisfyPrecondition(GoapEffect precondition) {
        for (int i = 0; i < expectedEffects.Count; i++) {
            if(EffectPreconditionMatching(expectedEffects[i], precondition)) {
                return true;
            }
        }
        return false;
    }
    private bool EffectPreconditionMatching(GoapEffect effect, GoapEffect precondition) {
        if(effect.targetPOI == precondition.targetPOI && effect.conditionType == precondition.conditionType) {
            if(effect.conditionKey != null && precondition.conditionKey != null) {
                switch (effect.conditionType) {
                    case GOAP_EFFECT_CONDITION.HAS_SUPPLY:
                        int effectInt = (int) effect.conditionKey;
                        int preconditionInt = (int) precondition.conditionKey;
                        return effectInt >= preconditionInt;
                    default:
                        return effect.conditionKey == precondition.conditionKey;
                        //case GOAP_EFFECT_CONDITION.HAS_ITEM:
                        //    string effectString = effect.conditionKey as string;
                        //    string preconditionString = precondition.conditionKey as string;
                        //    return effectString.ToLower() == preconditionString.ToLower();

                        //case GOAP_EFFECT_CONDITION.REMOVE_TRAIT:
                        //case GOAP_EFFECT_CONDITION.HAS_TRAIT:
                        //    effectString = effect.conditionKey as string;
                        //    preconditionString = precondition.conditionKey as string;
                        //    return effectString.ToLower() == preconditionString.ToLower();
                }
            } else {
                return true;
            }
        }
        return false;
    }
    public void AddActualEffect(GoapEffect effect) {
        actualEffects.Add(effect);
    }
    #endregion

    #region Tile Objects
    protected virtual void ReserveTarget() {
        if (poiTarget is TileObject) {
            TileObject target = poiTarget as TileObject;
            target.OnTargetObject(this);
            Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnActorDied);
        }
    }
    protected virtual void OnPerformActualActionToTarget() {
        if (poiTarget is TileObject) {
            TileObject target = poiTarget as TileObject;
            target.OnDoActionToObject(this);
        }
    }
    private void OnActorDied(Character character) {
        if (character.id == actor.id) {
            if (poiTarget is TileObject) {
                TileObject target = poiTarget as TileObject;
                target.SetPOIState(POI_STATE.ACTIVE); //this is for when the character that reserved the target object died
            }
        }
    }
    #endregion
}

public struct GoapEffect {
    public GOAP_EFFECT_CONDITION conditionType;
    public object conditionKey;
    public IPointOfInterest targetPOI; //this is the target that will be affected by the condition type and key

    public GoapEffect(GOAP_EFFECT_CONDITION conditionType, object conditionKey = null, IPointOfInterest targetPOI = null) {
        this.conditionType = conditionType;
        this.conditionKey = conditionKey;
        this.targetPOI = targetPOI;
    }

    public string conditionString() {
        if(conditionKey is string) {
            return conditionKey.ToString();
        } else if (conditionKey is int) {
            return conditionKey.ToString();
        } else if (conditionKey is Character) {
            return (conditionKey as Character).name;
        } else if (conditionKey is Area) {
            return (conditionKey as Area).name;
        }
        return string.Empty;
    }

    //public override bool Equals(object obj) {
    //    if (obj is GoapEffect) {
    //        GoapEffect otherEffect = (GoapEffect)obj;
    //        if (otherEffect.conditionType == conditionType) {
    //            if (string.IsNullOrEmpty(conditionString())) {
    //                return true;
    //            } else {
    //                return otherEffect.conditionString() == conditionString();
    //            }
    //        }
    //    }
    //    return base.Equals(obj);
    //}
    //public override int GetHashCode() {
    //    return base.GetHashCode();
    //}
}