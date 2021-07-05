using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Logs;
using UtilityScripts;
public class DestroyData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DESTROY;
    public override string name => "Destroy";
    public override string description => "This Ability instantly destroys an object." +
        "\nDestroying a resource pile from a Village city center will produce a Chaos Orb.";
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
        targetPOI.AdjustHP(-targetPOI.currentHP, ELEMENTAL_TYPE.Normal, true, isPlayerSource: true);
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "player_intervention", null, LOG_TAG.Player);
        log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, "destroyed", LOG_IDENTIFIER.STRING_1);
        log.AddLogToDatabase();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);

        if (UIManager.Instance.tileObjectInfoUI.isShowing && UIManager.Instance.tileObjectInfoUI.activeTileObject == targetPOI) {
            UIManager.Instance.tileObjectInfoUI.CloseMenu();
        }
        int processedDamage = PlayerSkillManager.Instance.GetDamageBaseOnLevel(this);
        float piercing = PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(this);
        List<LocationGridTile> tilesInRadius = RuinarchListPool<LocationGridTile>.Claim();
        targetTile.PopulateTilesInRadius(tilesInRadius, PlayerSkillManager.Instance.GetTileRangeBonusPerLevel(PLAYER_SKILL_TYPE.DESTROY));
        for (int i = 0; i < tilesInRadius.Count; i++) {
            LocationGridTile eachTile = tilesInRadius[i];
            if (eachTile != null) {
                GameManager.Instance.CreateParticleEffectAt(eachTile, PARTICLE_EFFECT.Destroy_Explosion);
                eachTile.charactersHere.ForEach((eachCharacters) => {
                    eachCharacters.AdjustHP(-processedDamage, ELEMENTAL_TYPE.Normal, true, showHPBar: true,
                        piercingPower: piercing, isPlayerSource: true);
                    Messenger.Broadcast(PlayerSignals.PLAYER_HIT_CHARACTER_VIA_SPELL, eachCharacters, processedDamage);
                    if (eachCharacters.isDead && eachCharacters.skillCauseOfDeath == PLAYER_SKILL_TYPE.NONE) {
                        eachCharacters.skillCauseOfDeath = PLAYER_SKILL_TYPE.DESTROY;
                        //if (eachCharacters.deathTilePosition != null) {
                        //    //Messenger.Broadcast(PlayerSignals.CREATE_SPIRIT_ENERGY, eachCharacters.deathTilePosition.centeredWorldLocation, 1, eachCharacters.deathTilePosition.parentMap);
                        //    //Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, eachCharacters.deathTilePosition.centeredWorldLocation, 1, eachCharacters.deathTilePosition.parentMap);
                        //}
                    }
                });
            }
        }
        RuinarchListPool<LocationGridTile>.Release(tilesInRadius);
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