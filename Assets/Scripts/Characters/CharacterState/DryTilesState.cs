using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;

public class DryTilesState : CharacterState {

    private LocationGridTile currentTarget;
    private bool _isDryingTile;
    
    public DryTilesState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Dry Tiles State";
        characterState = CHARACTER_STATE.DRY_TILES;
        duration = 0;
        actionIconString = GoapActionStateDB.Work_Icon;
    }
    
    #region Overrides
    protected override void StartState() {
        base.StartState();
        Messenger.AddListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
        DetermineAction();
    }
    protected override void EndState() {
        base.EndState();
        Messenger.RemoveListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
    }
    private void DetermineAction() {
        if (StillHasWetTile()) {
            //dry nearest wet tile
            DryNearestTile();
        } else {
            if (stateComponent.character.currentActionNode == null && stateComponent.currentState == this) {
                //no more wet tiles, exit state
                stateComponent.ExitCurrentState();
            }
        }
    }
    public override void PerTickInState() {
        DetermineAction();
    }
    public override void PauseState() {
        base.PauseState();
        if (_isDryingTile) {
            _isDryingTile = false;
        }
    }
    #endregion

    private bool StillHasWetTile() {
        return stateComponent.character.homeSettlement.settlementJobTriggerComponent.wetTiles.Count > 0;
    }
    private void DryNearestTile() {
        if (_isDryingTile) {
            return;
        }
        LocationGridTile nearestTile = null;
        float nearest = 99999f;
        if (currentTarget != null && currentTarget.genericTileObject.traitContainer.GetNormalTrait<Wet>("Wet") != null) {
            nearest = Vector2.Distance(stateComponent.character.worldObject.transform.position, currentTarget.worldLocation);
            nearestTile = currentTarget;
        }

        for (int i = 0; i < stateComponent.character.homeSettlement.settlementJobTriggerComponent.wetTiles.Count; i++) {
            LocationGridTile wetTile = stateComponent.character.homeSettlement.settlementJobTriggerComponent.wetTiles[i];
            Wet wet = wetTile.genericTileObject.traitContainer.GetNormalTrait<Wet>("Wet");
            if (wet != null && wet.dryer == null) {
                //only consider dousing fire that is not yet assigned
                float dist = Vector2.Distance(stateComponent.character.worldObject.transform.position, wetTile.worldLocation);
                if (dist < nearest) {
                    nearestTile = wetTile;
                    nearest = dist;
                }    
            }
        }
        if (nearestTile != null) {
            _isDryingTile = true;
            currentTarget = nearestTile;
            Wet wet = nearestTile.genericTileObject.traitContainer.GetNormalTrait<Wet>("Wet"); 
            Assert.IsNotNull(wet, $"Wet of {nearestTile} is null.");
            wet.SetDryer(stateComponent.character);
            stateComponent.character.marker.GoTo(nearestTile, Dry);
        } 
    }

    private void Dry() {
        if (currentTarget != null) {
            currentTarget.genericTileObject.traitContainer.RemoveStatusAndStacks(currentTarget.genericTileObject, "Wet", stateComponent.character);
            currentTarget = null;
        }
        _isDryingTile = false;
        DetermineAction();
    }
    
    private void OnTraitableLostTrait(ITraitable traitable, Trait trait, Character removedBy) {
        if (trait is Wet && traitable is GenericTileObject) {
            if (currentTarget == traitable.gridTileLocation) {
                currentTarget = null;
                _isDryingTile = false;
            }
        }
    }
}
