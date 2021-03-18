using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimiterComponent : CharacterComponent {
    public int canWitnessValue { get; private set; }//if this is >= 0 then character can witness events
    public int canMoveValue { get; private set; } //if this is >= 0 then character can move
    public int canBeAttackedValue { get; private set; } //if this is >= 0 then character can be attacked
    public int canPerformValue { get; private set; }//if this is >= 0 then character can perform
    public int canTakeJobsValue { get; private set; }//if this is >= 0 then character can take jobs
    public int sociableValue { get; private set; } //if this is >= 0 then character wants to socialize
    public int canDoFullnessRecoveryValue { get; private set; } //if this is >= 0 then character can do fullness recovery
    public int canDoHappinessRecoveryValue { get; private set; } //if this is >= 0 then character can do happiness recovery
    public int canDoTirednessRecoveryValue { get; private set; } //if this is >= 0 then character can do tiredness recovery

    #region getters
    public bool canWitness => canWitnessValue >= 0;
    public bool canMove => canMoveValue >= 0;
    public bool canBeAttacked => canBeAttackedValue >= 0;
    public bool canPerform => canPerformValue >= 0;
    public bool canTakeJobs => canTakeJobsValue >= 0;
    public bool isSociable => sociableValue >= 0;
    public bool canDoFullnessRecovery => canDoFullnessRecoveryValue >= 0;
    public bool canDoHappinessRecovery => canDoHappinessRecoveryValue >= 0;
    public bool canDoTirednessRecovery => canDoTirednessRecoveryValue >= 0;
    #endregion

    public LimiterComponent() {

    }
    public LimiterComponent(SaveDataLimiterComponent data) {
        ApplyDataFromSave(data);
    }

    public void IncreaseCanWitness() {
        canWitnessValue++;
    }
    public void DecreaseCanWitness() {
        canWitnessValue--;
    }
    public void IncreaseCanMove() {
        bool couldNotMoveBefore = canMove == false;
        canMoveValue++;
        if (couldNotMoveBefore && canMove) {
            //character could not move before adjustment, but can move after adjustment
            Messenger.Broadcast(CharacterSignals.CHARACTER_CAN_MOVE_AGAIN, owner);
        }
    }
    public void DecreaseCanMove() {
        bool couldMoveBefore = canMove;
        canMoveValue--;
        if (couldMoveBefore && canMove == false) {
            owner.partyComponent.UnfollowBeacon();
            //character could move before adjustment, but cannot move after adjustment
            Messenger.Broadcast(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, owner);
        }
    }
    public void IncreaseCanBeAttacked() {
        canBeAttackedValue++;
    }
    public void DecreaseCanBeAttacked() {
        canBeAttackedValue--;
    }
    public void IncreaseCanPerform() {
        bool couldNotPerformBefore = canPerform == false;
        canPerformValue++;
        if (couldNotPerformBefore && canPerform) {
            //character could not perform before adjustment, but can perform after adjustment
            Messenger.Broadcast(CharacterSignals.CHARACTER_CAN_PERFORM_AGAIN, owner);
        }
    }
    public void DecreaseCanPerform() {
        bool couldPerformBefore = canPerform;
        canPerformValue--;
        if (couldPerformBefore && canPerform == false) {
            owner.partyComponent.UnfollowBeacon();
            //character could perform before adjustment, but cannot perform after adjustment
            Messenger.Broadcast(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, owner);
        }
    }
    public void IncreaseCanTakeJobs() {
        canTakeJobsValue++;
    }
    public void DecreaseCanTakeJobs() {
        canTakeJobsValue--;
    }
    public void IncreaseSociable() {
        sociableValue++;
    }
    public void DecreaseSociable() {
        sociableValue--;
    }
    public void IncreaseCanDoFullnessRecovery() {
        canDoFullnessRecoveryValue++;
    }
    public void DecreaseCanDoFullnessRecovery() {
        canDoFullnessRecoveryValue--;
    }
    public void IncreaseCanDoHappinessRecovery() {
        canDoHappinessRecoveryValue++;
    }
    public void DecreaseCanDoHappinessRecovery() {
        canDoHappinessRecoveryValue--;
    }
    public void IncreaseCanDoTirednessRecovery() {
        canDoTirednessRecoveryValue++;
    }
    public void DecreaseCanDoTirednessRecovery() {
        canDoTirednessRecoveryValue--;
    }

    #region Loading
    public void ApplyDataFromSave(SaveDataLimiterComponent data) {
        canWitnessValue = data.canWitnessValue;
        canMoveValue = data.canMoveValue;
        canBeAttackedValue = data.canBeAttackedValue;
        canPerformValue = data.canPerformValue;
        canTakeJobsValue = data.canTakeJobsValue;
        sociableValue = data.sociableValue;
        canDoFullnessRecoveryValue = data.canDoFullnessRecoveryValue;
        canDoHappinessRecoveryValue = data.canDoHappinessRecoveryValue;
        canDoTirednessRecoveryValue = data.canDoTirednessRecoveryValue;
    }
    #endregion
}

[System.Serializable]
public class SaveDataLimiterComponent : SaveData<LimiterComponent> {
    public int canWitnessValue;
    public int canMoveValue;
    public int canBeAttackedValue;
    public int canPerformValue;
    public int canTakeJobsValue;
    public int sociableValue;
    public int canDoFullnessRecoveryValue;
    public int canDoHappinessRecoveryValue;
    public int canDoTirednessRecoveryValue;

    #region Overrides
    public override void Save(LimiterComponent data) {
        canWitnessValue = data.canWitnessValue;
        canMoveValue = data.canMoveValue;
        canBeAttackedValue = data.canBeAttackedValue;
        canPerformValue = data.canPerformValue;
        canTakeJobsValue = data.canTakeJobsValue;
        sociableValue = data.sociableValue;
        canDoFullnessRecoveryValue = data.canDoFullnessRecoveryValue;
        canDoHappinessRecoveryValue = data.canDoHappinessRecoveryValue;
        canDoTirednessRecoveryValue = data.canDoTirednessRecoveryValue;
    }

    public override LimiterComponent Load() {
        LimiterComponent component = new LimiterComponent(this);
        return component;
    }
    #endregion
}