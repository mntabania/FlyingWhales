using System.Collections.Generic;
using Inner_Maps;
using Scriptable_Object_Scripts;
using Traits;
using UtilityScripts;

public class WindBlastData : SkillData {

    PlayerSkillData m_playerSkillData;
    SkillData m_skillData;
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
        m_playerSkillData = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(PLAYER_SKILL_TYPE.WIND_BLAST);
        m_skillData = PlayerSkillManager.Instance.GetPlayerSkillData(PLAYER_SKILL_TYPE.WIND_BLAST);

        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Wind_Blast);
        int processedTileRange = m_baseTilerange + m_playerSkillData.skillUpgradeData.GetTileRangeBonusPerLevel(m_skillData.currentLevel);
        List<LocationGridTile> tiles = targetTile.GetTilesInRadius(processedTileRange, includeCenterTile: true, includeTilesInDifferentStructure: true);
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            tile.PerformActionOnTraitables(ApplyWindDamage);
        }
        targetTile.genericTileObject.traitContainer.AddTrait(targetTile.genericTileObject, "Danger Remnant");
        //IncreaseThreatThatSeesTile(targetTile, 10);
        base.ActivateAbility(targetTile);
    }
    private void ApplyWindDamage(ITraitable traitable) {
        int processedDamage = m_baseWindDamage - (m_baseWindDamage * m_playerSkillData.skillUpgradeData.GetAdditionalDamageBaseOnLevel(m_skillData.currentLevel));
        traitable.AdjustHP(processedDamage, ELEMENTAL_TYPE.Wind, true, showHPBar: true);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile) {
        bool canPerform = base.CanPerformAbilityTowards(targetTile);
        if (canPerform) {
            return targetTile.structure != null;
        }
        return canPerform;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(1, tile);
    }
}