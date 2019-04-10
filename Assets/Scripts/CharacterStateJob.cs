﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStateJob : JobQueueItem {

    public CHARACTER_STATE targetState { get; protected set; }
    public CharacterState assignedState { get; protected set; }

    public CharacterStateJob(CHARACTER_STATE state) {
        this.targetState = state;
    }

    #region Overrides
    public override void UnassignJob() {
        base.UnassignJob();
        if(assignedState != null && assignedCharacter != null) {
            if(assignedCharacter.stateComponent.currentState == assignedState) {
                assignedCharacter.stateComponent.currentState.OnExitThisState();
            } else {
                if(assignedCharacter.stateComponent.previousMajorState == assignedState) {
                    assignedCharacter.stateComponent.currentState.OnExitThisState();
                    if(assignedCharacter.stateComponent.currentState != null) {
                        //This happens because the character switched back to the previous major state
                        assignedCharacter.stateComponent.currentState.OnExitThisState();
                    }
                }
            }
        }
    }
    protected override bool CanTakeJob(Character character) {
        if(targetState == CHARACTER_STATE.PATROL && character.role.roleType == CHARACTER_ROLE.SOLDIER) {
            return true;
        }else if (targetState == CHARACTER_STATE.EXPLORE && character.role.roleType == CHARACTER_ROLE.ADVENTURER) {
            return true;
        }
        return false;
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
