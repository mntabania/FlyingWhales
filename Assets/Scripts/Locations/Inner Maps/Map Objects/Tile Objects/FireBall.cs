using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;

public class FireBall : MovingTileObject {

    private FireBallMapObjectVisual _fireBallMapVisual;
    public override string neutralizer => "Fire Master";
    public GameDate expiryDate { get; }
    
    public override System.Type serializedData => typeof(SaveDataFireBall);
    
    public FireBall() {
        Initialize(TILE_OBJECT_TYPE.FIRE_BALL, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        expiryDate = GameManager.Instance.Today().AddTicks((int)PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.FIRE_BALL));
    }
    public FireBall(SaveDataFireBall data) : base(data) {
        //SaveDataFireBall saveDataFireBall = data as SaveDataFireBall;
        Assert.IsNotNull(data);
        expiryDate = data.expiryDate;
        hasExpired = data.hasExpired;
    }
    protected override void CreateMapObjectVisual() {
        base.CreateMapObjectVisual();
        _fireBallMapVisual = mapVisual as FireBallMapObjectVisual;
        Assert.IsNotNull(_fireBallMapVisual, $"Map Object Visual of {this} is null!");
    }
    public override void Neutralize() {
        _fireBallMapVisual.Expire();
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        traitContainer.AddTrait(this, "Dangerous");
        traitContainer.RemoveTrait(this, "Flammable");
    }
    public void OnExpire() {
        Messenger.Broadcast<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, this, null, base.gridTileLocation);
    }
    public override string ToString() {
        return "Fire Ball";
    }
    public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false,
        object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false, float piercingPower = 0f, bool isPlayerSource = false) {
        if (currentHP == 0 && amount < 0) {
            return; //hp is already at minimum, do not allow any more negative adjustments
        }
        LocationGridTile tileLocation = gridTileLocation;
        CombatManager.Instance.ModifyDamage(ref amount, elementalDamageType, piercingPower, this);
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        if (amount < 0) { 
            Character responsibleCharacter = null;
            if (source is Character character) {
                responsibleCharacter = character;
            }
            //amount += -PlayerSkillManager.Instance.GetAdditionalDamageBaseOnLevel(PLAYER_SKILL_TYPE.FIRE_BALL);
            CombatManager.Instance.ApplyElementalDamage(amount, elementalDamageType, this, responsibleCharacter, elementalTraitProcessor, setAsPlayerSource: isPlayerSource);
            //Messenger.Broadcast(PlayerSignals.PLAYER_HIT_CHARACTER_VIA_SPELL, responsibleCharacter, amount);
            //if (responsibleCharacter != null && !responsibleCharacter.HasHealth()) {
            //    responsibleCharacter.skillCauseOfDeath = PLAYER_SKILL_TYPE.FIRE_BALL;
            //    Messenger.Broadcast(PlayerSignals.CREATE_SPIRIT_ENERGY, responsibleCharacter.deathTilePosition.centeredLocalLocation, 1, responsibleCharacter.currentRegion.innerMap);
            //}
        }
        if (amount < 0 && elementalDamageType == ELEMENTAL_TYPE.Water) {
            //2 Vapors
            for (int i = 0; i < 2; i++) {
                Vapor vapor = new Vapor();
                vapor.SetStacks(2);
                vapor.SetGridTileLocation(tileLocation);
                vapor.OnPlacePOI();
            }
        } else if (currentHP == 0 || (amount < 0 && elementalDamageType == ELEMENTAL_TYPE.Ice)) {
            //object has been destroyed
            _fireBallMapVisual.Expire();
        }
#if DEBUG_LOG
        Debug.Log($"{GameManager.Instance.TodayLogString()}HP of {this} was adjusted by {amount}. New HP is {currentHP}.");
#endif
    }

#region Moving Tile Object
    protected override bool TryGetGridTileLocation(out LocationGridTile tile) {
        if (_fireBallMapVisual != null) {
            if (_fireBallMapVisual.isSpawned) {
                tile = _fireBallMapVisual.gridTileLocation;
                return true;
            }
        }
        tile = null;
        return false;
    }
#endregion
}
#region Save Data
public class SaveDataFireBall : SaveDataMovingTileObject {
    public GameDate expiryDate;
    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        FireBall fireBall = tileObject as FireBall;
        Assert.IsNotNull(fireBall);
        expiryDate = fireBall.expiryDate;
    }
}
#endregion