using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStateJob : JobQueueItem {

    public CHARACTER_STATE targetState { get; protected set; }
    public CharacterState assignedState { get; protected set; }
    public Region targetRegion { get; protected set; }
    public IPointOfInterest targetPOI { get; protected set; }
    public List<System.Action<Character>> onUnassignActions { get; private set; }

    public CharacterStateJob() : base() {
        onUnassignActions = new List<System.Action<Character>>();
    }
    public void Initialize(JOB_TYPE jobType, CHARACTER_STATE state, Region targetRegion, IJobOwner owner) {
        Initialize(jobType, owner);
        this.targetState = state;
        this.targetRegion = targetRegion;
        //onUnassignActions = new List<System.Action<Character>>();
    }
    public void Initialize(JOB_TYPE jobType, CHARACTER_STATE state, IPointOfInterest targetPOI, IJobOwner owner) {
        Initialize(jobType, owner);
        this.targetState = state;
        this.targetPOI = targetPOI;
        //onUnassignActions = new List<System.Action<Character>>();
    }
    public void Initialize(JOB_TYPE jobType, CHARACTER_STATE state, IJobOwner owner) {
        Initialize(jobType, owner);
        this.targetState = state;
        //onUnassignActions = new List<System.Action<Character>>();
    }
    public void Initialize(SaveDataCharacterStateJob data) {
        Initialize(data);
        targetState = data.targetState;
        if(data.targetRegionID != -1) {
            targetRegion = GridMap.Instance.GetRegionByID(data.targetRegionID);
        } else {
            targetRegion = null;
        }
        //onUnassignActions = new List<System.Action<Character>>();
    }

    #region Overrides
    public override bool ProcessJob() {
        if (hasBeenReset) { return true; }
        if (assignedState == null) {
            CharacterState newState = assignedCharacter.stateComponent.SwitchToState(targetState, targetPOI);
            if (hasBeenReset) { return true; } //Need to check since job can be reset when the assignedCharacter switches states.
            //check if the new state is the assigned character's state, before assigning the state to this job.
            if (newState != null && assignedCharacter.stateComponent.currentState == newState) {
                SetAssignedState(newState);
                assignedCharacter.SetCurrentJob(this);
                if(newState is CombatState combatState) {
                    combatState.SetActionThatTriggeredThisState(assignedCharacter.combatComponent.actionThatTriggeredCombatState);
                    combatState.SetJobThatTriggeredThisState(assignedCharacter.combatComponent.jobThatTriggeredCombatState);
                    assignedCharacter.combatComponent.SetActionAndJobThatTriggeredCombat(null, null);
                }
                return true;
            } else {
                // throw new System.Exception(
                //     $"{assignedCharacter.name} tried doing state {targetState} but was unable to do so! This must not happen!");
                return false;
            }
        } else {
            if (assignedState.isDone) {
                CancelJob(false);
                return true;
            } else if(assignedState.isPaused) {
                assignedState.ResumeState();
                if (assignedState != null) {
                    if (!assignedState.isDone) {
                        assignedCharacter.SetCurrentJob(this);
                    } else {
                        if (assignedCharacter.currentJob == this) {
                            assignedCharacter.SetCurrentJob(null);
                        }
                    }
                    return true;    
                }
            }
        }
        return base.ProcessJob();
    }
    public override void PushedBack(JobQueueItem jobThatPushedBack) {
        if (cannotBePushedBack) {
            //If job is cannot be pushed back and it is pushed back, cancel it instead
            CancelJob(false);
        } else if (assignedState != null){
            assignedState.PauseState();
            assignedCharacter.stateComponent.SetCurrentState(null);
        }
    }
    public override void StopJobNotDrop() {
        if (cannotBePushedBack) {
            //If job is cannot be pushed back and it is stopped, cancel it
            CancelJob(false);
        } else if (assignedState != null) {
            assignedState.PauseState();
            assignedCharacter.stateComponent.SetCurrentState(null);
        }
    }
    public override void UnassignJob(bool shouldDoAfterEffect, string reason) {
        base.UnassignJob(shouldDoAfterEffect, reason);
        if(assignedCharacter != null) {
            //if(assignedCharacter.stateComponent.stateToDo == assignedState) {
            //    assignedCharacter.stateComponent.SetStateToDo(null);
            //}
            if(assignedState != null) {
                if (assignedCharacter.stateComponent.currentState == assignedState) {
                    Character character = assignedCharacter;
                    SetAssignedCharacter(null);
                    SetAssignedState(null);
                    character.stateComponent.ExitCurrentState();
                }
            }
            //else {
            //    if(assignedCharacter.stateComponent.previousMajorState == assignedState) {
            //        Character character = assignedCharacter;
            //        character.stateComponent.currentState.OnExitThisState();
            //        if(character.stateComponent.currentState != null) {
            //            //This happens because the character switched back to the previous major state
            //            character.stateComponent.currentState.OnExitThisState();
            //        }
            //    }
            //}
        }
    }
    //protected override bool CanTakeJob(Character character) {
    //    if(targetState == CHARACTER_STATE.PATROL) {
    //        if(character.role.roleType == CHARACTER_ROLE.SOLDIER) {
    //            return true;
    //        }
    //        return false;
    //    }
    //    return base.CanTakeJob(character);
    //}
    public override void OnCharacterUnassignedToJob(Character character) {
        base.OnCharacterAssignedToJob(character);
        ExecuteUnassignActions(character);
    }
    public override void Reset() {
        base.Reset();
        targetState = CHARACTER_STATE.NONE;
        assignedState = null;
        targetRegion = null;
        targetPOI = null;
        onUnassignActions.Clear();
    }
    #endregion

    public void SetAssignedState(CharacterState state) {
        if (state != null) {
            state.SetJob(this);
        }
        if (assignedState != null) {
            assignedState.SetJob(null);
        }
        assignedState = state;
    }

    #region Unassign Actions
    public void AddOnUnassignAction(System.Action<Character> unassignAction) {
        onUnassignActions.Add(unassignAction);
    }
    /// <summary>
    /// Execute external actions that are triggered when this job becomes unassigned. 
    /// NOTE: This also clears the list of said actions.
    /// </summary>
    private void ExecuteUnassignActions(Character unassignedCharacter) {
        for (int i = 0; i < onUnassignActions.Count; i++) {
            onUnassignActions[i].Invoke(unassignedCharacter);
        }
        onUnassignActions.Clear();
    }
    #endregion

}

public class SaveDataCharacterStateJob : SaveDataJobQueueItem {
    public CHARACTER_STATE targetState;
    public int targetRegionID;

    //Only save assigned character in state job because 
    public int assignedCharacterID;
    public SaveDataCharacterState connectedState; //This should only have value if the character's current state is connected to this job

    public override void Save(JobQueueItem job) {
        base.Save(job);
        CharacterStateJob stateJob = job as CharacterStateJob;
        targetState = stateJob.targetState;
        if(stateJob.targetRegion != null) {
            targetRegionID = stateJob.targetRegion.id;
        } else {
            targetRegionID = -1;
        }
        if(stateJob.assignedCharacter != null) {
            assignedCharacterID = stateJob.assignedCharacter.id;
            if (stateJob.assignedCharacter.stateComponent.currentState.job == stateJob) {
                connectedState = SaveUtilities.CreateCharacterStateSaveDataInstance(stateJob.assignedCharacter.stateComponent.currentState);
                connectedState.Save(stateJob.assignedCharacter.stateComponent.currentState);
            }
        } else {
            assignedCharacterID = -1;
        }
    }

    public override JobQueueItem Load() {
        CharacterStateJob stateJob = base.Load() as CharacterStateJob;
        if (this.assignedCharacterID != -1) {
            Character assignedCharacter = CharacterManager.Instance.GetCharacterByID(this.assignedCharacterID);
            stateJob.SetAssignedCharacter(assignedCharacter);
            //Load Character State if there is any.
            if (this.connectedState != null) {
                CharacterState newState = assignedCharacter.stateComponent.LoadState(connectedState);
                if (newState != null) {
                    stateJob.SetAssignedState(newState);
                } else {
                    throw new System.Exception(
                        $"{assignedCharacter.name} tried doing state {stateJob.targetState} but was unable to do so! This must not happen!");
                }
            }
        }

        return stateJob;
    }


}