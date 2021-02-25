using System.Collections.Generic;
using Inner_Maps;
using Scriptable_Object_Scripts;
using Traits;
using UtilityScripts;

public class WindBlastData : SkillData {

    private int m_baseWindDamage = -500;
    private int m_baseTilerange = 1;
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.WIND_BLAST;
    public override string name => "Wind Blast";
    public override string description => "This Spell blasts a powerful wave of air outward from a target spot, dealing Wind damage to anything it hits.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

    public WindBlastData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        AudioManager.Instance.TryCreateAudioObject(
            PlayerSkillManager.Instance.GetPlayerSkillData<WindBlastSkillData>(PLAYER_SKILL_TYPE.WIND_BLAST).blastSound,
            targetTile, 3, false
        );

        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Wind_Blast);
        int processedTileRange = m_baseTilerange + PlayerSkillManager.Instance.GetTileRangeBonusPerLevel(PLAYER_SKILL_TYPE.WIND_BLAST);
        List<LocationGridTile> tiles = targetTile.GetTilesInRadius(processedTileRange, includeCenterTile: true, includeTilesInDifferentStructure: true);
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            tile.PerformActionOnTraitables(ApplyWindDamage);
        }
        targetTile.tileObjectComponent.genericTileObject.traitContainer.AddTrait(targetTile.tileObjectComponent.genericTileObject, "Danger Remnant");
        //IncreaseThreatThatSeesTile(targetTile, 10);
        base.ActivateAbility(targetTile);
    }
    private void ApplyWindDamage(ITraitable traitable) {
        int processedDamage = m_baseWindDamage - (m_baseWindDamage * PlayerSkillManager.Instance.GetAdditionalDamageBaseOnLevel(PLAYER_SKILL_TYPE.WIND_BLAST));
        traitable.AdjustHP(processedDamage, ELEMENTAL_TYPE.Wind, true, showHPBar: true);

        if (traitable is Character character) {
            Messenger.Broadcast(PlayerSignals.PLAYER_HIT_CHARACTER_VIA_SPELL, character, processedDamage);
            if (character.currentHP <= 0){
                character.skillCauseOfDeath = PLAYER_SKILL_TYPE.WIND_BLAST;
                Messenger.Broadcast(PlayerSignals.CREATE_SPIRIT_ENERGY, character.marker.transform.position, 1, character.currentRegion.innerMap);
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
        TileHighlighter.Instance.PositionHighlight(1, tile);
    }
}