using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;

public class LocustSwarmTileObject : MovingTileObject {

    private LocustSwarmMapObjectVisual _locustSwarmMapObjectVisual;
    
    public LocustSwarmTileObject() {
        Initialize(TILE_OBJECT_TYPE.LOCUST_SWARM, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        traitContainer.AddTrait(this, "Dangerous");
    }
    protected override void CreateMapObjectVisual() {
        base.CreateMapObjectVisual();
        _locustSwarmMapObjectVisual = mapVisual as LocustSwarmMapObjectVisual;
        Assert.IsNotNull(_locustSwarmMapObjectVisual, $"Map Object Visual of {this} is null!");
    }
    public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false, object source = null) {
        if (currentHP == 0 && amount < 0) {
            return; //hp is already at minimum, do not allow any more negative adjustments
        }
        CombatManager.Instance.DamageModifierByElements(ref amount, elementalDamageType, this);
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        if (amount < 0 && source != null) {
            //CombatManager.Instance.CreateHitEffectAt(this, elementalDamageType);
            Character responsibleCharacter = null;
            if (source is Character character) {
                responsibleCharacter = character;
            }
            CombatManager.Instance.ApplyElementalDamage(amount, elementalDamageType, this, responsibleCharacter);
            
        }
        if (currentHP == 0) {
            //object has been destroyed
            _locustSwarmMapObjectVisual.Expire();
        }
        if (amount < 0) {
            Messenger.Broadcast(Signals.OBJECT_DAMAGED, this as IPointOfInterest);    
        } else if (currentHP == maxHP) {
            Messenger.Broadcast(Signals.OBJECT_REPAIRED, this as IPointOfInterest);
        }
        Debug.Log($"{GameManager.Instance.TodayLogString()}HP of {this} was adjusted by {amount}. New HP is {currentHP}.");
    }
    public override void Neutralize() {
        _locustSwarmMapObjectVisual.Expire();
    }
}