﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Logs;

public class DestroyData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DESTROY;
    public override string name => "Destroy";
    public override string description => "This Action destroys an object.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.PLAYER_ACTION;
    public DestroyData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE_OBJECT };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        LocationGridTile targetTile = targetPOI.gridTileLocation;
        
        if (targetTile != null) {
            GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Destroy_Explosion);    
        }
        targetPOI.AdjustHP(-targetPOI.currentHP, ELEMENTAL_TYPE.Normal, true);
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "player_intervention", null, LOG_TAG.Player);
        log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, "destroyed", LOG_IDENTIFIER.STRING_1);
        log.AddLogToDatabase();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);

        if (UIManager.Instance.tileObjectInfoUI.isShowing && UIManager.Instance.tileObjectInfoUI.activeTileObject == targetPOI) {
            UIManager.Instance.tileObjectInfoUI.CloseMenu();
        }

        targetTile.GetTilesInRadius(PlayerSkillManager.Instance.GetTileRangeBonusPerLevel(PLAYER_SKILL_TYPE.DESTROY)).ForEach((eachTile) => {
            if (eachTile != null) {
                GameManager.Instance.CreateParticleEffectAt(eachTile, PARTICLE_EFFECT.Destroy_Explosion);
                eachTile.charactersHere.ForEach((eachCharacters) => {
                    int processedDamage = PlayerSkillManager.Instance.GetAdditionalDamageBaseOnLevel(PLAYER_SKILL_TYPE.DESTROY);
                    eachCharacters.AdjustHP(processedDamage * -1, ELEMENTAL_TYPE.Normal, showHPBar: true,
                        piercingPower: PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.DESTROY));
                    Messenger.Broadcast(PlayerSignals.PLAYER_HIT_CHARACTER_VIA_SPELL, eachCharacters, processedDamage);
                    if (!eachCharacters.HasHealth()) {
                        eachCharacters.skillCauseOfDeath = PLAYER_SKILL_TYPE.DESTROY;
                        Messenger.Broadcast(PlayerSignals.CREATE_SPIRIT_ENERGY, eachCharacters.deathTilePosition.centeredLocalLocation, 1, eachCharacters.currentRegion.innerMap);
                    }
                });
            }
        });

        base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(TileObject tileObject) {
        if (tileObject.gridTileLocation == null) {
            return false;
        }
        if (tileObject.isBeingCarriedBy != null) {
            return false;
        }
        return base.CanPerformAbilityTowards(tileObject);
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if(target is IPointOfInterest poi && poi.traitContainer.HasTrait("Indestructible")) {
            return false;
        }
        return base.IsValid(target);
    }
    #endregion
}