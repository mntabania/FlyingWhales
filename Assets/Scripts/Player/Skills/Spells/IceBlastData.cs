﻿using System.Collections.Generic;
using Inner_Maps;
using Scriptable_Object_Scripts;
using Traits;
using UtilityScripts;

public class IceBlastData : SkillData {

    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.ICE_BLAST;
    public override string name => "Ice Blast";
    public override string description => "This Spell causes cold spikes to shatter around a small area, dealing Ice damage to those within range.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

    public IceBlastData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        AudioManager.Instance.TryCreateAudioObject(
            PlayerSkillManager.Instance.GetPlayerSkillData<IceBlastSkillData>(PLAYER_SKILL_TYPE.ICE_BLAST).blastSound,
            targetTile, 3, false
        );

        int processedTileRange = PlayerSkillManager.Instance.GetTileRangeBonusPerLevel(PLAYER_SKILL_TYPE.ICE_BLAST);
        UnityEngine.GameObject go = GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Ice_Blast);
        go.transform.localScale = new UnityEngine.Vector3(go.transform.localScale.x * processedTileRange, go.transform.localScale.y * processedTileRange, go.transform.localScale.z * processedTileRange);
        List<LocationGridTile> tiles = targetTile.GetTilesInRadius(processedTileRange, includeCenterTile: true, includeTilesInDifferentStructure: true);
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            tile.PerformActionOnTraitables(ApplyIceDamage);
        }
        targetTile.tileObjectComponent.genericTileObject.traitContainer.AddTrait(targetTile.tileObjectComponent.genericTileObject, "Danger Remnant");
        //IncreaseThreatThatSeesTile(targetTile, 10);
        base.ActivateAbility(targetTile);
    }
    private void ApplyIceDamage(ITraitable traitable) {
        int processedDamage = (-PlayerSkillManager.Instance.GetDamageBaseOnLevel(PLAYER_SKILL_TYPE.ICE_BLAST));
        traitable.AdjustHP(processedDamage, ELEMENTAL_TYPE.Ice, true, showHPBar: true, isPlayerSource: true);

        if (traitable is Character character) {
            Messenger.Broadcast(PlayerSignals.PLAYER_HIT_CHARACTER_VIA_SPELL, character, processedDamage);
            if (character.isDead && character.skillCauseOfDeath == PLAYER_SKILL_TYPE.NONE) {
                character.skillCauseOfDeath = PLAYER_SKILL_TYPE.ICE_BLAST;
                Messenger.Broadcast(PlayerSignals.CREATE_SPIRIT_ENERGY, character.deathTilePosition.centeredWorldLocation, 1, character.deathTilePosition.parentMap);
            }
        }
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason) {
        bool canPerform = base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason);
        if (canPerform) {
            return targetTile.structure != null;
        }
        return canPerform;
    }
    public override void ShowValidHighlight(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(PlayerSkillManager.Instance.GetTileRangeBonusPerLevel(PLAYER_SKILL_TYPE.ICE_BLAST), tile);
    }
}