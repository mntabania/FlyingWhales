using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterState {
    public CharacterStateComponent stateComponent { get; protected set; }
    public string stateName { get; protected set; }
    public CHARACTER_STATE characterState { get; protected set; }
    //public CHARACTER_STATE_CATEGORY stateCategory { get; protected set; }
    public int duration { get; protected set; } // 0 means no duration - permanent
    public int currentDuration { get; protected set; }
    //public int level { get; protected set; } //Right now, only used in berserk to know what level BerserkBuff will be
    public bool isDone { get; protected set; }
    public bool hasStarted { get; protected set; }
    public bool isPaused { get; protected set; }
    public Log thoughtBubbleLog { get; protected set; }
    public CharacterStateJob job { get; protected set; }
    public string actionIconString { get; protected set; }
    public IPointOfInterest targetPOI { get; protected set; }
    //public GoapAction currentlyDoingAction { get; protected set; }
    //public Character targetCharacter { get; protected set; } //Target character of current state
    //public NPCSettlement targetNpcSettlement { get; protected set; }
    //public bool isUnending { get; protected set; } //is this state unending?
    //public CharacterState parentMajorState { get; protected set; }

    //public System.Action startStateAction { get; protected set; }
    //public System.Action endStateAction { get; protected set; }

    public CharacterState(CharacterStateComponent characterComp) {
        this.stateComponent = characterComp;
        actionIconString = GoapActionStateDB.No_Icon;
        //isUnending = false;
        //AddDefaultListeners();
    }

    #region Virtuals
    public virtual void Load(SaveDataCharacterState saveData) {
        this.SetCurrentDuration(saveData.currentDuration);
        //this.SetIsUnending(saveData.isUnending);
        //if (saveData.targetCharacterID != -1) {
        //    Character targetCharacter = CharacterManager.Instance.GetCharacterByID(saveData.targetCharacterID);
        //    this.SetTargetCharacter(targetCharacter);
        //}
        //if (saveData.targetAreaID != -1) {
        //    NPCSettlement targetNpcSettlement = LandmarkManager.Instance.GetAreaByID(saveData.targetAreaID);
        //    this.SetTargetArea(targetNpcSettlement);
        //}
    }
    //Starts a state and its movement behavior, can be overridden
    protected virtual void StartState() {
        hasStarted = true;
        //stateComponent.SetStateToDo(null, false, false);
        currentDuration = 0;
        //StartStatePerTick();
        stateComponent.SetCurrentState(this);
        CreateStartStateLog();
        CreateThoughtBubbleLog();
        DoMovementBehavior();
        Messenger.Broadcast(Signals.CHARACTER_STARTED_STATE, stateComponent.character, this);
        ProcessInVisionPOIsOnStartState();
        //if(startStateAction != null) {
        //    startStateAction();
        //}
        //stateComponent.character.JustDropAllPlansOfType(INTERACTION_TYPE.WATCH);
    }
    /// <summary>
    /// End this state. This is called after <see cref="OnExitThisState"/>.
    /// </summary>
    protected virtual void EndState() {
        //removed this, nothing sets currentlyDoingAction anymore.
        //if (currentlyDoingAction != null) {
        //    if (currentlyDoingAction.isPerformingActualAction && !currentlyDoingAction.isDone) {
        //        currentlyDoingAction.SetEndAction(FakeEndAction);
        //        currentlyDoingAction.currentState.EndPerTickEffect(false);
        //    }
        //    stateComponent.character.SetCurrentActionNode(null);
        //    SetCurrentlyDoingAction(null);
        //}
        isDone = true;
        //StopStatePerTick();
        //RemoveDefaultListeners();
        //if(job != null) {
        //    //job.SetAssignedCharacter(null);
        //    //job.SetAssignedState(null);
        //    //job.assignedCharacter.jobQueue.RemoveJobInQueue(job);
        //    job.ForceCancelJob();
        //}
        // Messenger.Broadcast(Signals.CHARACTER_ENDED_STATE, stateComponent.character, this);
    }
    //Only call this base function if state has duration
    public virtual void PerTickInState() {
        currentDuration++;
        //if (!isPaused /*&& !isUnending*/ && !isDone) {
        //    if (currentDuration >= duration) {
        //        StopStatePerTick();
        //        OnExitThisState();
        //    } else if (stateComponent.character.doNotDisturb > 0) {
        //        StopStatePerTick();
        //        OnExitThisState();
        //    }
        //    currentDuration++;
        //}
    }
    //Character will do the movement behavior of this state, can be overriden
    protected virtual void DoMovementBehavior() {}
    //What happens when you see another point of interest (character, tile objects, etc)
    public virtual bool OnEnterVisionWith(IPointOfInterest targetPOI) { return false; }
    //What happens if there are already point of interest in your vision upon entering the state
    public virtual bool ProcessInVisionPOIsOnStartState() {
        if(stateComponent.character.marker.inVisionPOIs.Count > 0) {
            return true;
        }
        return false;
    }
    //This is called for exiting current state, I made it a virtual because some states still requires something before exiting current state
    //public virtual void OnExitThisState() {
    //    stateComponent.ExitCurrentState(this);
    //}
    //Typically used if there are other data that is needed to be set for this state when it starts
    //Currently used only in combat state so we can set the character's behavior if attacking or not when it enters the state
    //public virtual void SetOtherDataOnStartState(object otherData) { }
    //This is called on ExitCurrentState function in CharacterStateComponent after all exit processing is finished
    public virtual void AfterExitingState() {
        stateComponent.character.marker.UpdateActionIcon();
        Messenger.Broadcast(Signals.CHARACTER_ENDED_STATE, stateComponent.character, this);
    }
    //public virtual bool CanResumeState() {
    //    return true;
    //}
    /// <summary>
    /// Pauses this state, used in switching states if this is a major state
    /// </summary>
    public virtual void PauseState() {
        stateComponent.character.logComponent.PrintLogIfActive(
            $"Pausing {stateName} for {stateComponent.character.name}");
        isPaused = true;
        //StopStatePerTick();
        Messenger.Broadcast(Signals.CHARACTER_PAUSED_STATE, stateComponent.character, this);
    }
    /// <summary>
    /// Resumes the state and its movement behavior
    /// </summary>
    public virtual void ResumeState() {
        if (isDone) {
            return; //if the state has already been exited. Do not resume.
        }
        if (!isPaused) {
            return; //if this state is not paused then do not resume.
        }
        stateComponent.character.logComponent.PrintLogIfActive(
            $"Resuming {stateName} for {stateComponent.character.name}");
        isPaused = false;
        stateComponent.SetCurrentState(this);
        //StartStatePerTick();
        DoMovementBehavior();
    }
    protected virtual void OnJobSet() { }
    protected virtual void CreateThoughtBubbleLog() {
        if (LocalizationManager.Instance.HasLocalizedValue("CharacterState", stateName, "thought_bubble")) {
            thoughtBubbleLog = new Log(GameManager.Instance.Today(), "CharacterState", stateName, "thought_bubble");
            thoughtBubbleLog.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            if (targetPOI != null) {
                thoughtBubbleLog.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.TARGET_CHARACTER); //Target character is only the identifier but it doesn't mean that this is a character, it can be item, etc.
            }
        }
    }
    #endregion

    //private void FakeEndAction(string str, GoapAction action) {
    //    //This is just a fake holder end action so that the currently doing action will not go to its actual end action (ex. PatrolAgain)
    //    //This is done because we don't want the GoapActionResult to be called as well as the actual end action
    //}

    //Stops the timer of this state
    //public void StopStatePerTick() {
    //    if (Messenger.eventTable.ContainsKey(Signals.TICK_ENDED)) {
    //        Messenger.RemoveListener(Signals.TICK_ENDED, PerTickInState);
    //    }
    //}
    //Starts the timer of this state
    //public void StartStatePerTick() {
    //    //if(duration > 0) {
    //        Messenger.AddListener(Signals.TICK_ENDED, PerTickInState);
    //    //}
    //}
    //Sets the target character of this state, if there's any
    //public void SetTargetCharacter(Character target) {
    //    targetCharacter = target;
    //}
    ////Sets the target npcSettlement of this state, if there's any
    //public void SetTargetArea(NPCSettlement target) {
    //    targetNpcSettlement = target;
    //}
    //This is the action that is currently being done while in this state, ex. pick up item
    //public void SetCurrentlyDoingAction(GoapAction action) {
    //    currentlyDoingAction = action;
    //}
    //public void SetParentMajorState(CharacterState majorState) {
    //    parentMajorState = majorState;
    //}
    //This is the one must be called to enter and start this state, if it is already done, it cannot start again
    public void EnterState() {
        if (isDone) {
            return;
        }
        //stateComponent.SetStateToDo(this, stopMovement: false);
        //stateComponent.character.PrintLogIfActive(GameManager.Instance.TodayLogString() + "Entering " + stateName + " for " + stateComponent.character.name + " targetting " + targetCharacter?.name);
        StartState();
        ////targetNpcSettlement = npcSettlement;
        //if (targetNpcSettlement == null || targetNpcSettlement == stateComponent.character.specificLocation) {
        //    stateComponent.character.PrintLogIfActive(GameManager.Instance.TodayLogString() + "Entering " + stateName + " for " + stateComponent.character.name + " targetting " + targetCharacter?.name);
        //    StartState();
        //} else {
        //    //GameDate dueDate = GameManager.Instance.Today().AddTicks(30);
        //    //SchedulingManager.Instance.AddEntry(dueDate, () => GoToLocation(targetNpcSettlement));
        //    CreateTravellingThoughtBubbleLog(targetNpcSettlement);
        //    stateComponent.character.PrintLogIfActive(GameManager.Instance.TodayLogString() + "Travelling to " + targetNpcSettlement.name + " before entering " + stateName + " for " + stateComponent.character.name);
        //    stateComponent.character.currentParty.GoToLocation(targetNpcSettlement.region, PATHFINDING_MODE.NORMAL, null, () => StartState());
        //}
        //if(characterState == CHARACTER_STATE.EXPLORE) {
        //    //There is a special case for explore state, character must travel to a dungeon-type npcSettlement first
        //    NPCSettlement dungeon = LandmarkManager.Instance.GetRandomAreaOfType(AREA_TYPE.DUNGEON);
        //    if(dungeon == stateComponent.character.specificLocation) {
        //        Debug.Log(GameManager.Instance.TodayLogString() + "Entering " + stateName + " for " + stateComponent.character.name);
        //        StartState();
        //    } else {
        //        CreateTravellingThoughtBubbleLog(dungeon);
        //        Debug.Log(GameManager.Instance.TodayLogString() + "Travelling to " + dungeon.name + " before entering " + stateName + " for " + stateComponent.character.name);
        //        stateComponent.character.currentParty.GoToLocation(dungeon, PATHFINDING_MODE.NORMAL, null, () => StartState());
        //    }
        //} else {
        //    Debug.Log(GameManager.Instance.TodayLogString() + "Entering " + stateName + " for " + stateComponent.character.name);
        //    StartState();
        //}
    }
    //private void GoToLocation(NPCSettlement targetNpcSettlement) {
    //    CreateTravellingThoughtBubbleLog(targetNpcSettlement);
    //    Debug.Log(GameManager.Instance.TodayLogString() + "Travelling to " + targetNpcSettlement.name + " before entering " + stateName + " for " + stateComponent.character.name);
    //    stateComponent.character.currentParty.GoToLocation(targetNpcSettlement, PATHFINDING_MODE.NORMAL, null, () => StartState());
    //}
    //This is the one must be called to exit and end this state
    public void ExitState() {
        stateComponent.character.logComponent.PrintLogIfActive(
            $"Exiting {stateName} for {stateComponent.character.name}" /*+ " targetting " + targetCharacter?.name ?? "No One"*/);
        EndState();
    }
    public void SetJob(CharacterStateJob job) {
        this.job = job;
        if (job != null) {
            OnJobSet();
        } else {
            Debug.Log($"{GameManager.Instance.TodayLogString()}{this} Set job to null!");
        }
    }
    /// <summary>
    /// What should happen once the job of this state is set to anything other than null?
    /// </summary>
    private void CreateStartStateLog() {
        if (LocalizationManager.Instance.HasLocalizedValue("CharacterState", stateName, "start")) {
            Log log = new Log(GameManager.Instance.Today(), "CharacterState", stateName, "start");
            log.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            if (targetPOI != null) {
                log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.TARGET_CHARACTER); //Target character is only the identifier but it doesn't mean that this is a character, it can be item, etc.
            }
            //if(targetNpcSettlement != null) {
            //    log.AddToFillers(targetNpcSettlement, targetNpcSettlement.name, LOG_IDENTIFIER.LANDMARK_1);
            //}
            log.AddLogToInvolvedObjects();

            // PlayerManager.Instance.player.ShowNotificationFrom(log, stateComponent.character, false);
        }
    }
    private void CreateTravellingThoughtBubbleLog(NPCSettlement targetLocation) {
        if (LocalizationManager.Instance.HasLocalizedValue("CharacterState", stateName, "thought_bubble_m")) {
            thoughtBubbleLog = new Log(GameManager.Instance.Today(), "CharacterState", stateName, "thought_bubble_m");
            thoughtBubbleLog.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            thoughtBubbleLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_1);
        }
    }
    public void SetTargetPOI(IPointOfInterest poi) {
        targetPOI = poi;
    }
    //public void SetStartStateAction(System.Action action) {
    //    startStateAction = action;
    //}
    //public void SetEndStateAction(System.Action action) {
    //    endStateAction = action;
    //}

    //#region Listeners
    //private void AddDefaultListeners() {
    //    Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
    //}
    //private void RemoveDefaultListeners() {
    //    Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
    //}
    ////handler for when the character that owns this dies
    //private void OnCharacterDied(Character character) {
    //    if (character.id == stateComponent.character.id) {
    //        StopStatePerTick();
    //        RemoveDefaultListeners();
    //    }
    //}
    //#endregion

    #region Utilities
    internal void ChangeDuration(int newDuration) {
        duration = newDuration;
    }
    public void SetCurrentDuration(int amount) {
        currentDuration = amount;
    }
    /// <summary>
    /// Set if this state only has a specific duration, or will it run indefinitely until stopped.
    /// </summary>
    /// <param name="state">If the state should be unending or not.</param>
    //public void SetIsUnending(bool state) {
    //    isUnending = state;
    //}
    public override string ToString() {
        return $"{stateName} by {stateComponent.character.name} with job : {(job?.name ?? "None")}";
    }
    #endregion
}

//Combat and Character State with Jobs must not be saved since they have separate process for that
//Combat is entered when there is hostile or avoid in range
//Character States with Job are processed in their respective Jobs
[System.Serializable]
public class SaveDataCharacterState {
    public CHARACTER_STATE characterState;
    public int duration;
    public int currentDuration;
    public bool isPaused;
    public int targetCharacterID;
    //public int targetAreaID;
    public bool isUnending;
    public bool hasStarted;
    public int level;

    public virtual void Save(CharacterState state) {
        characterState = state.characterState;
        duration = state.duration;
        currentDuration = state.currentDuration;
        isPaused = state.isPaused;
        //isUnending = state.isUnending;
        hasStarted = state.hasStarted;
        //level = state.level;

        //if(state.targetCharacter != null) {
        //    targetCharacterID = state.targetCharacter.id;
        //} else {
        //    targetCharacterID = -1;
        //}
        //if (state.targetNpcSettlement != null) {
        //    targetAreaID = state.targetNpcSettlement.id;
        //} else {
        //    targetAreaID = -1;
        //}
    }

    //public virtual CharacterState Load(Character character) {
    //    CharacterState state = character.stateComponent.CreateNewState(characterState);
    //    if(targetCharacterID != -1) {
    //        state.SetTargetCharacter(CharacterManager.Instance.GetCharacterByID(targetCharacterID));
    //    }
    //    if (targetAreaID != -1) {
    //        state.SetTargetArea(LandmarkManager.Instance.GetAreaByID(targetAreaID));
    //    }
    //    state.ChangeDuration(duration);
    //    state.SetCurrentDuration(currentDuration);
    //    state.SetIsUnending(isUnending);
    //    state.EnterState();
    //}
}