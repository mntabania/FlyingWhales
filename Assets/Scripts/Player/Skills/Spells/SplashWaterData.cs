using System.Collections.Generic;
using Inner_Maps;
using Traits;

public class SplashWaterData : SkillData {

    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SPLASH_WATER;
    public override string name => "Splash Water";
    public override string description => "This Spell applies Wet to a 3x3 tile floor.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

    public int m_baseTileRange = 1;

    public SplashWaterData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }
    public override void ActivateAbility(LocationGridTile targetTile) {
        int processedTileRange = m_baseTileRange + PlayerSkillManager.Instance.GetTileRangeBonusPerLevel(PLAYER_SKILL_TYPE.SPLASH_WATER);
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
        traitable.traitContainer.AddTrait(traitable, "Wet", bypassElementalChance: true, overrideDuration: PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.SPLASH_WATER));
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

