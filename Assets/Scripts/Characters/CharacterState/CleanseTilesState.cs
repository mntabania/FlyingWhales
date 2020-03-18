using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;

public class CleanseTilesState : CharacterState {

    private LocationGridTile currentTarget;
    private bool _isCleansingTile;
    private bool _isCleansingWithoutIce;
    private int _noIceTicks;
    
    public CleanseTilesState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Cleanse Tiles State";
        characterState = CHARACTER_STATE.CLEANSE_TILES;
        duration = 0;
        actionIconString = GoapActionStateDB.Clean_Icon;
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
        if (StillHasPoisonedTile()) {
            //cleanse nearest poisoned tile
            CleanseNearestTile();
        } else {
            if (stateComponent.character.currentActionNode == null && stateComponent.currentState == this) {
                //no more poisoned tiles, exit state
                stateComponent.ExitCurrentState();
            }
        }
    }
    public override void PerTickInState() {
        DetermineAction();
        if (_isCleansingWithoutIce) {
            _noIceTicks++;
            if (_noIceTicks > 3) {
                _noIceTicks = 0;
                Cleanse();
            }
        }
    }
    public override void PauseState() {
        base.PauseState();
        if (_isCleansingTile) {
            _isCleansingTile = false;
            currentTarget = null;
        }
    }
    #endregion

    private bool StillHasPoisonedTile() {
        return stateComponent.character.homeSettlement.settlementJobTriggerComponent.poisonedTiles.Count > 0;
    }
    private void CleanseNearestTile() {
        if (_isCleansingTile) {
            return;
        }
        LocationGridTile nearestTile = null;
        float nearest = 99999f;
        if (currentTarget != null && currentTarget.genericTileObject.traitContainer.GetNormalTrait<Poisoned>("Poisoned") != null) {
            nearest = Vector2.Distance(stateComponent.character.worldObject.transform.position, currentTarget.worldLocation);
            nearestTile = currentTarget;
        }

        for (int i = 0; i < stateComponent.character.homeSettlement.settlementJobTriggerComponent.poisonedTiles.Count; i++) {
            LocationGridTile tile = stateComponent.character.homeSettlement.settlementJobTriggerComponent.poisonedTiles[i];
            Poisoned poisoned = tile.genericTileObject.traitContainer.GetNormalTrait<Poisoned>("Poisoned");
            if (poisoned != null && poisoned.cleanser == null) {
                float dist = Vector2.Distance(stateComponent.character.worldObject.transform.position, tile.worldLocation);
                if (dist < nearest) {
                    nearestTile = tile;
                    nearest = dist;
                }    
            }
        }
        if (nearestTile != null) {
            _isCleansingTile = true;
            currentTarget = nearestTile;
            Poisoned poisoned = nearestTile.genericTileObject.traitContainer.GetNormalTrait<Poisoned>("Poisoned"); 
            Assert.IsNotNull(poisoned, $"Poisoned of {nearestTile} is null.");
            poisoned.SetCleanser(stateComponent.character);
            stateComponent.character.marker.GoTo(nearestTile, TryCleanse);
        } 
    }

    private void TryCleanse() {
        if (stateComponent.character.HasItem(TILE_OBJECT_TYPE.ICE_CRYSTAL)) {
            _isCleansingWithoutIce = false;
            Cleanse();
            stateComponent.character.UnobtainItem(TILE_OBJECT_TYPE.ICE_CRYSTAL);
        } else {
            _isCleansingWithoutIce = true;
        }
    }
    private void Cleanse() {
        if (currentTarget != null) {
            currentTarget.genericTileObject.traitContainer.RemoveStatusAndStacks(currentTarget.genericTileObject, "Poisoned", stateComponent.character);
            currentTarget = null;
            _isCleansingTile = false;
            DetermineAction();    
        }
    }
    
    private void OnTraitableLostTrait(ITraitable traitable, Trait trait, Character removedBy) {
        if (trait is Poisoned && traitable is GenericTileObject) {
            if (currentTarget == traitable.gridTileLocation) {
                currentTarget = null;
                _isCleansingTile = false;
            }
        }
    }
}
