using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class FollowState : CharacterState {

    private MovingMapObjectVisual<TileObject> _targetMapVisual;

    public FollowState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Follow State";
        characterState = CHARACTER_STATE.FOLLOW;
        duration = GameManager.Instance.GetTicksBasedOnHour(3);
    }

    #region Overrides
    protected override void StartState() {
        base.StartState();
        _targetMapVisual = targetPOI.mapObjectVisual as MovingMapObjectVisual<TileObject>;
    }
    protected override void DoMovementBehavior() {
        base.DoMovementBehavior();
        StartFollowMovement();
    }
    public override void PerTickInState() {
        if(_targetMapVisual == null || !_targetMapVisual.isSpawned) {
            stateComponent.ExitCurrentState();
        }
    }
    #endregion

    private void StartFollowMovement() {
        stateComponent.character.marker.GoTo(targetPOI, StartFollowMovement, null);
    }
}

