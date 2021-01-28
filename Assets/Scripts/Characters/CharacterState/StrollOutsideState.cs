using System.Collections;
using Inner_Maps.Location_Structures;
using UnityEngine;

public class StrollOutsideState : CharacterState {

    public StrollOutsideState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Stroll Outside State";
        characterState = CHARACTER_STATE.STROLL_OUTSIDE;
        duration = GameManager.ticksPerHour;
    }

    #region Overrides
    protected override void DoMovementBehavior() {
        base.DoMovementBehavior();
        StartStrollMovement();
    }
    #endregion

    public void StartStrollMovement() {
        stateComponent.owner.marker.DoStrollMovement();
    }
}
