using System.Collections.Generic;
using Inner_Maps;
using Traits;

public class SplashWaterData : SkillData {

    PlayerSkillData m_playerSkillData;
    SkillData m_skillData;

    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SPLASH_WATER;
    public override string name => "Splash Water";
    public override string description => "This Spell applies Wet to a 3x3 tile floor.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

    public int m_baseTileRange = 1;

    public SplashWaterData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }
    public override void ActivateAbility(LocationGridTile targetTile) {
        m_playerSkillData = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(PLAYER_SKILL_TYPE.SPLASH_WATER);
        m_skillData = PlayerSkillManager.Instance.GetPlayerSkillData(PLAYER_SKILL_TYPE.SPLASH_WATER);
        int processedTileRange = m_baseTileRange + m_playerSkillData.skillUpgradeData.GetTileRangeBonusPerLevel(m_skillData.currentLevel);
        List<LocationGridTile> tiles = targetTile.GetTilesInRadius(processedTileRange, includeCenterTile: true, includeTilesInDifferentStructure: true);
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            tile.PerformActionOnTraitables(MakeTraitbleWet);
        }
        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Water_Bomb);
        targetTile.genericTileObject.traitContainer.AddTrait(targetTile.genericTileObject, "Surprised Remnant");
        //IncreaseThreatThatSeesTile(targetTile, 10);
        base.ActivateAbility(targetTile);
    }
    private void MakeTraitbleWet(ITraitable traitable) {
        traitable.traitContainer.AddTrait(traitable, "Wet", bypassElementalChance: true, overrideDuration: m_playerSkillData.skillUpgradeData.GetDurationBonusPerLevel(m_skillData.currentLevel));
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

