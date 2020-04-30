using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;

public class LocustSwarmTileObject : MovingTileObject {

    private LocustSwarmMapObjectVisual _locustSwarmMapObjectVisual;
    public override string neutralizer => "Beastmaster";
    
    public LocustSwarmTileObject() {
        Initialize(TILE_OBJECT_TYPE.LOCUST_SWARM, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    protected override void CreateMapObjectVisual() {
        base.CreateMapObjectVisual();
        _locustSwarmMapObjectVisual = mapVisual as LocustSwarmMapObjectVisual;
        Assert.IsNotNull(_locustSwarmMapObjectVisual, $"Map Object Visual of {this} is null!");
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        traitContainer.AddTrait(this, "Dangerous");
    }
    public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false,
        object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false) {
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
            CombatManager.ElementalTraitProcessor etp = elementalTraitProcessor ?? 
                                                        CombatManager.Instance.DefaultElementalTraitProcessor;
            CombatManager.Instance.ApplyElementalDamage(amount, elementalDamageType, this, 
                responsibleCharacter, etp);
        }
        if (currentHP == 0) {
            //object has been destroyed
            _locustSwarmMapObjectVisual.Expire();
        }
        if (amount < 0) {
            Messenger.Broadcast(Signals.OBJECT_DAMAGED, this as IPointOfInterest, amount);
        } else if (amount > 0) {
            if (currentHP == maxHP) {
                Messenger.Broadcast(Signals.OBJECT_FULLY_REPAIRED, this as IPointOfInterest);
            } else {
                Messenger.Broadcast(Signals.OBJECT_REPAIRED, this as IPointOfInterest, amount);
            }
        }
        Debug.Log($"{GameManager.Instance.TodayLogString()}HP of {this} was adjusted by {amount}. New HP is {currentHP}.");
    }
    public override void Neutralize() {
        _locustSwarmMapObjectVisual.Expire();
    }
}