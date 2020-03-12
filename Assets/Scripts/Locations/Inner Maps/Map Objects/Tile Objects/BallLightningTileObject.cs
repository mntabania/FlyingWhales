using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;

public class BallLightningTileObject : MovingTileObject {

    private BallLightningMapObjectVisual _ballLightningMapVisual;
    
    public BallLightningTileObject() {
        Initialize(TILE_OBJECT_TYPE.BALL_LIGHTNING, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        traitContainer.AddTrait(this, "Dangerous");
        traitContainer.RemoveTrait(this, "Flammable");
    }
    protected override void CreateMapObjectVisual() {
        base.CreateMapObjectVisual();
        _ballLightningMapVisual = mapVisual as BallLightningMapObjectVisual;
        Assert.IsNotNull(_ballLightningMapVisual, $"Map Object Visual of {this} is null!");
    }
    public override void Neutralize() {
        _ballLightningMapVisual.Expire();
    }
    public void OnExpire() {
        Messenger.Broadcast<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, this, null, base.gridTileLocation);
    }
    public override string ToString() {
        return "Ball Lightning";
    }
    public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false, object source = null) {
        if (currentHP == 0 && amount < 0) {
            return; //hp is already at minimum, do not allow any more negative adjustments
        }
        LocationGridTile tileLocation = gridTileLocation;
        CombatManager.Instance.DamageModifierByElements(ref amount, elementalDamageType, this);
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        if (amount < 0) { //&& source != null
            //CombatManager.Instance.CreateHitEffectAt(this, elementalDamageType);
            Character responsibleCharacter = null;
            if (source is Character character) {
                responsibleCharacter = character;
            }
            CombatManager.Instance.ApplyElementalDamage(amount, elementalDamageType, this, responsibleCharacter);
        }
        if (amount < 0 && elementalDamageType == ELEMENTAL_TYPE.Ice) {
            //Electric Storm
            if (tileLocation.buildSpotOwner.hexTileOwner != null) {
                tileLocation.buildSpotOwner.hexTileOwner.spellsComponent.SetHasElectricStorm(true);
            }
            _ballLightningMapVisual.Expire();
        } else if (currentHP == 0) {
            //object has been destroyed
            _ballLightningMapVisual.Expire();
        }
        //if (amount < 0) {
        //    Messenger.Broadcast(Signals.OBJECT_DAMAGED, this as IPointOfInterest);
        //} else if (currentHP == maxHP) {
        //    Messenger.Broadcast(Signals.OBJECT_REPAIRED, this as IPointOfInterest);
        //}
        Debug.Log($"{GameManager.Instance.TodayLogString()}HP of {this} was adjusted by {amount}. New HP is {currentHP}.");
    }

    #region Moving Tile Object
    protected override bool TryGetGridTileLocation(out LocationGridTile tile) {
        if (_ballLightningMapVisual != null) {
            if (_ballLightningMapVisual.isSpawned) {
                tile = _ballLightningMapVisual.gridTileLocation;
                return true;
            }
        }
        tile = null;
        return false;
    }
    #endregion
}
