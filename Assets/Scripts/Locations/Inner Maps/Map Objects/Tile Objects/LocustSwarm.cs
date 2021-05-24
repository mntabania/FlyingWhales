using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;

public class LocustSwarm : MovingTileObject {

    private LocustSwarmMapObjectVisual _locustSwarmMapObjectVisual;
    public override string neutralizer => "Beastmaster";
    public GameDate expiryDate { get; }
    
    public override System.Type serializedData => typeof(SaveDataLocustSwarm);
    
    public LocustSwarm() {
        int processedTick = PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.LOCUST_SWARM);
        Initialize(TILE_OBJECT_TYPE.LOCUST_SWARM, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        expiryDate = GameManager.Instance.Today().AddTicks(processedTick);
    }
    public LocustSwarm(SaveDataLocustSwarm data) : base(data) {
        //SaveDataLocustSwarm saveDataLocustSwarm = data as SaveDataLocustSwarm;
        Assert.IsNotNull(data);
        expiryDate = data.expiryDate;
        hasExpired = data.hasExpired;
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
        object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false, float piercingPower = 0f, bool isPlayerSource = false) {
        if (currentHP == 0 && amount < 0) {
            return; //hp is already at minimum, do not allow any more negative adjustments
        }
        CombatManager.Instance.ModifyDamage(ref amount, elementalDamageType, piercingPower, this);
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        if (amount < 0 && source != null) {
            //CombatManager.Instance.CreateHitEffectAt(this, elementalDamageType);
            Character responsibleCharacter = null;
            if (source is Character character) {
                responsibleCharacter = character;
            }
            CombatManager.Instance.ApplyElementalDamage(amount, elementalDamageType, this, responsibleCharacter, elementalTraitProcessor, setAsPlayerSource: isPlayerSource);
        }
        if (currentHP == 0) {
            //object has been destroyed
            _locustSwarmMapObjectVisual.Expire();
        }
        if (amount < 0) {
            if (source is Character responsibleCharacter) {
                Messenger.Broadcast(TileObjectSignals.TILE_OBJECT_DAMAGED_BY, this as TileObject, amount, responsibleCharacter, isPlayerSource);
            } else {
                Messenger.Broadcast(TileObjectSignals.TILE_OBJECT_DAMAGED, this as TileObject, amount, isPlayerSource);
            }
        } else if (amount > 0) {
            if (currentHP == maxHP) {
                Messenger.Broadcast(TileObjectSignals.TILE_OBJECT_FULLY_REPAIRED, this as TileObject);
            } else {
                Messenger.Broadcast(TileObjectSignals.TILE_OBJECT_REPAIRED, this as TileObject, amount);
            }
        }
#if DEBUG_LOG
        Debug.Log($"{GameManager.Instance.TodayLogString()}HP of {this} was adjusted by {amount}. New HP is {currentHP}.");
#endif
    }
    public override void Neutralize() {
        _locustSwarmMapObjectVisual.Expire();
    }
}

#region Save Data
public class SaveDataLocustSwarm : SaveDataMovingTileObject {
    public GameDate expiryDate;
    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        LocustSwarm locustSwarm = tileObject as LocustSwarm;
        Assert.IsNotNull(locustSwarm);
        expiryDate = locustSwarm.expiryDate;
    }
}
#endregion