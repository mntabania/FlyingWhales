using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;

public class BallLightning : MovingTileObject {

    private BallLightningMapObjectVisual _ballLightningMapVisual;
    public override string neutralizer => "Thunder Master";
    public GameDate expiryDate { get; }
    
    public override System.Type serializedData => typeof(SaveDataBallLightning);
    
    public BallLightning() {
        Initialize(TILE_OBJECT_TYPE.BALL_LIGHTNING, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        expiryDate = GameManager.Instance.Today().AddTicks(PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.BALL_LIGHTNING));
    }
    public BallLightning(SaveDataBallLightning data) : base(data) {
        //SaveDataBallLightning saveDataBallLightning = data as SaveDataBallLightning;
        Assert.IsNotNull(data);
        expiryDate = data.expiryDate;
        hasExpired = data.hasExpired;
    }
    protected override void CreateMapObjectVisual() {
        base.CreateMapObjectVisual();
        _ballLightningMapVisual = mapVisual as BallLightningMapObjectVisual;
        Assert.IsNotNull(_ballLightningMapVisual, $"Map Object Visual of {this} is null!");
    }
    public override void Neutralize() {
        _ballLightningMapVisual.Expire();
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        traitContainer.AddTrait(this, "Dangerous");
        traitContainer.RemoveTrait(this, "Flammable");
    }
    public override string ToString() {
        return "Ball Lightning";
    }
    public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false,
        object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false, float piercingPower = 0f, bool isPlayerSource = false) {
        if (currentHP == 0 && amount < 0) {
            return; //hp is already at minimum, do not allow any more negative adjustments
        }
        //amount += -PlayerSkillManager.Instance.GetAdditionalDamageBaseOnLevel(PLAYER_SKILL_TYPE.BALL_LIGHTNING);
        LocationGridTile tileLocation = gridTileLocation;
        CombatManager.Instance.ModifyDamage(ref amount, elementalDamageType, piercingPower, this);
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        if (amount < 0) { 
            Character responsibleCharacter = null;
            if (source is Character character) {
                responsibleCharacter = character;
            }
            CombatManager.Instance.ApplyElementalDamage(amount, elementalDamageType, this, responsibleCharacter, elementalTraitProcessor, setAsPlayerSource: isPlayerSource);
            //Messenger.Broadcast(PlayerSignals.PLAYER_HIT_CHARACTER_VIA_SPELL, responsibleCharacter, amount);
            //if (responsibleCharacter != null && !responsibleCharacter.HasHealth()) {
            //    responsibleCharacter.skillCauseOfDeath = PLAYER_SKILL_TYPE.BALL_LIGHTNING;
            //    Messenger.Broadcast(PlayerSignals.CREATE_SPIRIT_ENERGY, responsibleCharacter.deathTilePosition.centeredLocalLocation, 1, responsibleCharacter.currentRegion.innerMap);
            //}
        }
        if (amount < 0 && elementalDamageType == ELEMENTAL_TYPE.Ice) {
            //Electric Storm
            tileLocation.area.spellsComponent.SetHasElectricStorm(true);
            
            _ballLightningMapVisual.Expire();
        } else if (currentHP == 0) {
            //object has been destroyed
            _ballLightningMapVisual.Expire();
        }
#if DEBUG_LOG
        Debug.Log($"{GameManager.Instance.TodayLogString()}HP of {this} was adjusted by {amount}. New HP is {currentHP}.");
#endif
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

    #region Reactions
    public override void GeneralReactionToTileObject(Character actor, ref string debugLog) {
        base.GeneralReactionToTileObject(actor, ref debugLog);
        if (actor is Troll) {
            if (traitContainer.HasTrait("Lightning Remnant")) {
                actor.combatComponent.Flight(this, "saw something frightening");
            }
        }
    }
    #endregion
}

#region Save Data
public class SaveDataBallLightning : SaveDataMovingTileObject {
    public GameDate expiryDate;
    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        BallLightning ballLightning = tileObject as BallLightning;
        Assert.IsNotNull(ballLightning);
        expiryDate = ballLightning.expiryDate;
    }
}
#endregion