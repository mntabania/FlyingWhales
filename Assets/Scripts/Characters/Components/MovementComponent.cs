using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementComponent {
    public Character owner { get; private set; }

    public bool isRunning { get; private set; }
    public bool noRunExceptCombat { get; private set; }
    public bool noRunWithoutException { get; private set; }
    public int useWalkSpeed { get; private set; }
    public float speedModifier { get; private set; }
    public float walkSpeedModifier { get; private set; }
    public float runSpeedModifier { get; private set; }

    #region getters
    public float walkSpeed => owner.raceSetting.walkSpeed + (owner.raceSetting.walkSpeed * walkSpeedModifier);
    public float runSpeed => owner.raceSetting.runSpeed + (owner.raceSetting.runSpeed * runSpeedModifier);
    #endregion

    public MovementComponent(Character owner) {
        this.owner = owner;
    }

    public void UpdateSpeed() {
        if (owner.marker) {
            SetMovementState();
            owner.marker.pathfindingAI.speed = GetSpeed();
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
        SetIsRunning(true);
        if (useWalkSpeed > 0 || noRunWithoutException || (noRunExceptCombat && !owner.isInCombat)) {
            SetIsRunning(false);
            return;
        } else {
            if (owner.stateComponent.currentState != null) {
                if (owner.stateComponent.currentState.characterState == CHARACTER_STATE.PATROL
                    || owner.stateComponent.currentState.characterState == CHARACTER_STATE.STROLL
                    || owner.stateComponent.currentState.characterState == CHARACTER_STATE.STROLL_OUTSIDE) {
                    //Walk
                    SetIsRunning(false);
                    return;
                }
            }
            if (owner.currentActionNode != null) {
                if (owner.currentActionNode.action.goapType == INTERACTION_TYPE.PATROL) {
                    SetIsRunning(false);
                    return;
                }
            }
        }
    }
    public void AdjustUseWalkSpeed(int amount) {
        useWalkSpeed += amount;
        useWalkSpeed = Mathf.Max(0, useWalkSpeed);
    }
}
