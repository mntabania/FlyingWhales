using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;

public class VaporTileObject : MovingTileObject {

    private VaporMapObjectVisual _vaporMapVisualObject;
    private int _stacks;
    
    public VaporTileObject() {
        Initialize(TILE_OBJECT_TYPE.VAPOR, false);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        traitContainer.RemoveTrait(this, "Flammable");
    }
    protected override void CreateMapObjectVisual() {
        base.CreateMapObjectVisual();
        _vaporMapVisualObject = mapVisual as VaporMapObjectVisual;
        _vaporMapVisualObject.SetSize(GetSizeFromStacks());
        Assert.IsNotNull(_vaporMapVisualObject, $"Map Object Visual of {this} is null!");
    }
    public void SetStacks(int stacks) {
        _stacks = stacks;
    }
    private int GetSizeFromStacks() {
        if(_stacks >= 1 && _stacks <= 2) {
            return 1;
        } else if (_stacks >= 3 && _stacks <= 4) {
            return 2;
        } else if (_stacks >= 5 && _stacks <= 9) {
            return 3;
        } else if (_stacks >= 10 && _stacks <= 16) {
            return 4;
        } else if (_stacks >= 17 && _stacks <= 25) {
            return 5;
        } else if (_stacks >= 26) {
            return 6;
        }
        throw new System.Exception("Getting size of Vapor based on stacks but stack " + _stacks + " is invalid!");
    }
    //public override void Neutralize() {
    //    _vaporMapVisualObject.Expire();
    //}
    public void OnExpire() {
        Messenger.Broadcast<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, this, null, base.gridTileLocation);
    }
    public override string ToString() {
        return "Vapor";
    }
    public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false,
        object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null) {
        if (currentHP == 0 && amount < 0) {
            return; //hp is already at minimum, do not allow any more negative adjustments
        }
        CombatManager.Instance.DamageModifierByElements(ref amount, elementalDamageType, this);
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        if (amount < 0) { //&& source != null
            //CombatManager.Instance.CreateHitEffectAt(this, elementalDamageType);
            Character responsibleCharacter = null;
            if (source is Character character) {
                responsibleCharacter = character;
            }
            CombatManager.ElementalTraitProcessor etp = elementalTraitProcessor ?? 
                                                        CombatManager.Instance.DefaultElementalTraitProcessor;
            CombatManager.Instance.ApplyElementalDamage(amount, elementalDamageType, this, 
                responsibleCharacter, etp);
        }
        if (elementalDamageType == ELEMENTAL_TYPE.Ice || currentHP == 0) {
            _vaporMapVisualObject.Expire();
        }
        Debug.Log($"{GameManager.Instance.TodayLogString()}HP of {this} was adjusted by {amount}. New HP is {currentHP}.");
    }

    #region Moving Tile Object
    protected override bool TryGetGridTileLocation(out LocationGridTile tile) {
        if (_vaporMapVisualObject != null) {
            if (_vaporMapVisualObject.isSpawned) {
                tile = _vaporMapVisualObject.gridTileLocation;
                return true;
            }
        }
        tile = null;
        return false;
    }
    #endregion
}
