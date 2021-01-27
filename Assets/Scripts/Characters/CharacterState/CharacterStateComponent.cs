using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

//This is the bridge between the character and the states, a component that stores all necessary information and process all data for the character and its states  
//Basically, this is a manager for the character states
//This is where you switch from one state to another, etc.
//Everything a character wants to do with a state must go through here
public class CharacterStateComponent : CharacterComponent {
    //If a major state is replaced by a minor state, must be stored in order for the character to go back to this state after doing the minor state
    //public CharacterState previousMajorState { get; private set; }
    //This is the character's current state
    public CharacterState currentState { get; private set; }
    

    public void OnTickEnded() {
        PerTickCurrentState();
    }
    public void SetCurrentState(CharacterState state) {
        if(currentState != state) {
            currentState = state;
            if (owner.marker) {
                owner.marker.UpdateActionIcon();
            }
        }
    }

    public CharacterStateComponent() {

    }
    public CharacterStateComponent(SaveDataCharacterStateComponent data) {

    }
   
    //This switches from one state to another
    //If the character is not in a state right now, this simply starts a new state instead of switching
    public CharacterState SwitchToState(CHARACTER_STATE state, IPointOfInterest targetPOI = null, int durationOverride = -1) {
        //Cannot switch state is has negative disabler
        if(!owner.limiterComponent.canPerform) { //character.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)
            return null;
        }
        //Before switching character must end current action first because once a character is in a state in cannot make plans
        if (owner.currentActionNode != null) { // && character.currentActionNode.action.goapType.ShouldBeStoppedWhenSwitchingStates() //removed this because it is no longer needed
            owner.StopCurrentActionNode();
        }

        //Stop the movement of character because the new state probably has different movement behavior
        if (owner.marker && owner.marker.isMoving) {
            owner.marker.StopMovement();
        }

        if (currentState != null) {
            ExitCurrentState();
        }
        //Create the new state
        CharacterState newState = CreateNewState(state);

        //Assigns new state as the current state then enter that state
        //newState.SetParentMajorState(previousMajorState);
        //currentState = newState;
        if (durationOverride != -1) {
            newState.ChangeDuration(durationOverride);
        }
        newState.SetTargetPOI(targetPOI);
        newState.EnterState();
        return newState;
    }
    /// <summary>
    /// Load a Character State given save data. 
    /// NOTE: This will also make the character enter the loaded state.
    /// </summary>
    /// <param name="saveData">Save data to load.</param>
    /// <returns>The state that was loaded.</returns>
    public CharacterState LoadState(SaveDataCharacterState saveData) {
        CharacterState loadedState = CreateNewState(saveData.characterState);
        loadedState.Load(saveData);
        loadedState.EnterState();
        return loadedState;
    }

    /// <summary>
    /// This ends the current state.
    /// This is triggered when the timer is out, or the character simply wants to end its state and go back to normal state.
    /// </summary>
    /// <param name="state">The state to be exited.</param>
    /// <param name="stopMovement">Should this character stop his/her current movement when exiting his/her current state?/param>
    public void ExitCurrentState() {
        if (currentState == null) {
            throw new System.Exception($"{owner.name} is trying to exit his/her current state but it is null");
        }

        if (owner.marker && owner.marker.isMoving) {
            owner.marker.StopMovement();
        }

        CharacterStateJob stateJob = currentState.job;
        CharacterState currState = currentState;
        currState.ExitState();
        SetCurrentState(null);
        currState.AfterExitingState();

        //Note: Original owner is checked here because there are times that the job for this state is already cancelled in AfterExitingState
        //So if we do not check here it will result in Null Ref since there is no more job at this point
        if(stateJob != null && stateJob.originalOwner != null) {
            if(stateJob == owner.currentJob) {
                owner.SetCurrentJob(null);
            }
            stateJob.ForceCancelJob();
        }

        if(currState.characterState == CHARACTER_STATE.COMBAT) {
            List<Trait> traitOverrideFunctions = owner.traitContainer.GetTraitOverrideFunctions(TraitManager.After_Exiting_Combat);
            if (traitOverrideFunctions != null) {
                for (int i = 0; i < traitOverrideFunctions.Count; i++) {
                    Trait trait = traitOverrideFunctions[i];
                    trait.OnAfterExitingCombat(owner);
                }
            }
            if (owner.isInWerewolfForm) {
                if (!owner.crimeComponent.HasNonHostileVillagerInRangeThatConsidersCrimeTypeACrime(CRIME_TYPE.Werewolf)) {
                    owner.interruptComponent.TriggerInterrupt(INTERRUPT.Revert_From_Werewolf, owner);
                } else {
                    owner.crimeComponent.FleeToAllNonHostileVillagerInRangeThatConsidersCrimeTypeACrime(owner, CRIME_TYPE.Werewolf);
                }
            }
        }
    }
    private void PerTickCurrentState() {
        if(currentState != null && !currentState.isPaused && !currentState.isDone) {
            if(!owner.limiterComponent.canPerform) {
                ExitCurrentState();
                return;
            }
            if(currentState.duration > 0) {
                //Current state has duration
                if (currentState.currentDuration >= currentState.duration) {
                    ExitCurrentState();
                    return;
                }
            }
            currentState.PerTickInState();
        }
    }

    public CharacterState CreateNewState(CHARACTER_STATE state) {
        CharacterState newState = null;
        switch (state) {
            case CHARACTER_STATE.PATROL:
                newState = new PatrolState(this);
                break;
            case CHARACTER_STATE.HUNT:
                newState = new HuntState(this);
                break;
            case CHARACTER_STATE.STROLL:
                newState = new StrollState(this);
                break;
            case CHARACTER_STATE.STROLL_OUTSIDE:
                newState = new StrollOutsideState(this);
                break;
            case CHARACTER_STATE.BERSERKED:
                newState = new BerserkedState(this);
                break;
            case CHARACTER_STATE.COMBAT:
                newState = new CombatState(this);
                break;
            case CHARACTER_STATE.DOUSE_FIRE:
                newState = new DouseFireState(this);
                break;
            case CHARACTER_STATE.FOLLOW:
                newState = new FollowState(this);
                break;
            case CHARACTER_STATE.DRY_TILES:
                newState = new DryTilesState(this);
                break;
            case CHARACTER_STATE.CLEANSE_TILES:
                newState = new CleanseTilesState(this);
                break;
        }
        return newState;
    }

    #region Loading
    public void LoadReferences(SaveDataCharacterStateComponent data) {
        //Currently N/A
    }
    #endregion
}

[System.Serializable]
public class SaveDataCharacterStateComponent : SaveData<CharacterStateComponent> {
    //Do not save current state? Just let it run on load?

    #region Overrides
    public override void Save(CharacterStateComponent data) {

    }

    public override CharacterStateComponent Load() {
        CharacterStateComponent component = new CharacterStateComponent(this);
        return component;
    }
    #endregion
}