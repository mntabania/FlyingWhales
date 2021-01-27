using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

public class CharacterStateJob : JobQueueItem {
    
    public CHARACTER_STATE targetState { get; protected set; }
    public CharacterState assignedState { get; protected set; }
    public IPointOfInterest targetPOI { get; protected set; }

    #region getters
    public override IPointOfInterest poiTarget => targetPOI;
    public override OBJECT_TYPE objectType => OBJECT_TYPE.Job;
    public override Type serializedData => typeof(SaveDataCharacterStateJob);
    #endregion
    
    public CharacterStateJob() : base() { }
    public void Initialize(JOB_TYPE jobType, CHARACTER_STATE state, IPointOfInterest targetPOI, IJobOwner owner) {
        Initialize(jobType, owner);
        this.targetState = state;
        this.targetPOI = targetPOI;
    }
    public void Initialize(JOB_TYPE jobType, CHARACTER_STATE state, IJobOwner owner) {
        Initialize(jobType, owner);
        this.targetState = state;
    }
    public void Initialize(SaveDataCharacterStateJob data) {
        base.Initialize(data);
        targetState = data.targetState;
    }

    #region Overrides
    //Returns true if we don't want to perform top prio, false if we want the character to perform top prio job after this
    public override bool ProcessJob() {
        if (hasBeenReset) { return true; }
        if (targetState == CHARACTER_STATE.COMBAT && assignedCharacter.combatComponent.hostilesInRange.Count <= 0 && assignedCharacter.combatComponent.avoidInRange.Count <= 0) {
            //Added a checker here because there are times that the combat job still persist even though the hostile/avoid list is empty
            //So if this happens, we must cancel job
            CancelJob(false);
            return true;
        }
        if (assignedState == null) {
            Profiler.BeginSample($"Character State Job - Process Job - Switch To State");
            CharacterState newState = assignedCharacter.stateComponent.SwitchToState(targetState, targetPOI);
            Profiler.EndSample();
            if (hasBeenReset) { return true; } //Need to check since job can be reset when the assignedCharacter switches states.
            //check if the new state is the assigned character's state, before assigning the state to this job.
            if (newState != null && assignedCharacter.stateComponent.currentState == newState) {
                SetAssignedState(newState);
                assignedCharacter.SetCurrentJob(this);
                //if(newState is CombatState combatState) {
                //    combatState.SetActionThatTriggeredThisState(assignedCharacter.combatComponent.actionThatTriggeredCombatState);
                //    combatState.SetJobThatTriggeredThisState(assignedCharacter.combatComponent.jobThatTriggeredCombatState);
                //    assignedCharacter.combatComponent.SetActionAndJobThatTriggeredCombat(null, null);
                //}
                return true;
            } else {
                // throw new System.Exception(
                //     $"{assignedCharacter.name} tried doing state {targetState} but was unable to do so! This must not happen!");
                return false;
            }
        } else {
            if (assignedState.isDone) {
                Profiler.BeginSample($"Character State Job - Process Job - Cancel Job - isDone");
                CancelJob(false);
                Profiler.EndSample();
                return true;
            } else if(assignedState.isPaused) {
                Profiler.BeginSample($"Character State Job - Process Job - Resume State - {assignedState.stateName}");
                assignedState.ResumeState();
                Profiler.EndSample();
                if (assignedState != null) {
                    if (assignedState.isDone && assignedCharacter.currentJob == this) {
                        assignedCharacter.SetCurrentJob(null);
                    }
                    return true;    
                }
            }
        }
        return base.ProcessJob();
    }
    public override void PushedBack(JobQueueItem jobThatPushedBack) {
        if (!cannotBePushedBack || jobThatPushedBack.jobType == JOB_TYPE.DIG_THROUGH) {
            if (assignedState != null) {
                assignedState.PauseState();
                //assignedCharacter.stateComponent.SetCurrentState(null);
            }
        } else {
            //If job is cannot be pushed back and it is pushed back, cancel it instead
            CancelJob(false);
        }
    }
    public override void StopJobNotDrop() {
        if (cannotBePushedBack) {
            //If job is cannot be pushed back and it is stopped, cancel it
            CancelJob(false);
        } else if (assignedState != null) {
            assignedState.PauseState();
            //assignedCharacter.stateComponent.SetCurrentState(null);
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
    public override bool CancelJob(bool shouldDoAfterEffect = true, string reason = "") {
        if(assignedState != null && assignedState.characterState == CHARACTER_STATE.COMBAT) {
            if(assignedCharacter != null) {
                if(assignedState.isPaused && !assignedState.isDone) {
                    //Once we cancel combat and it is currently paused, we need to resume it so that it will be properly cancelled, since while paused, we remove the assigned state as the current state of the character
                    assignedState.ResumeState();
                }
                assignedCharacter?.combatComponent.ClearHostilesInRange();
                assignedCharacter?.combatComponent.ClearAvoidInRange();
            }
        }
        return base.CancelJob(shouldDoAfterEffect, reason);
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
    public override void Reset() {
        base.Reset();
        targetState = CHARACTER_STATE.NONE;
        assignedState = null;
        targetPOI = null;
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
}

public class SaveDataCharacterStateJob : SaveDataJobQueueItem {
    public CHARACTER_STATE targetState;

    public override void Save(JobQueueItem job) {
        base.Save(job);
        CharacterStateJob stateJob = job as CharacterStateJob;
        Assert.IsNotNull(stateJob);
        targetState = stateJob.targetState;
    }
    public override JobQueueItem Load() {
        return JobManager.Instance.CreateNewCharacterStateJob(this);
    }
}