using System.Collections.Generic;
using Inner_Maps;
using Traits;

public class WindBlastData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.WIND_BLAST;
    public override string name => "Wind Blast";
    public override string description => "A powerful blast of wind spreads out from the target tile, dealing Wind damage to all in range.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.SPELL;

    public WindBlastData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Wind_Blast);
        List<LocationGridTile> tiles = targetTile.GetTilesInRadius(1, includeCenterTile: true, includeTilesInDifferentStructure: true);
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            tile.PerformActionOnTraitables(ApplyWindDamage);
        }
        IncreaseThreatThatSeesTile(targetTile, 10);
        base.ActivateAbility(targetTile);
    }
    private void ApplyWindDamage(ITraitable traitable) {
        traitable.AdjustHP(-300, ELEMENTAL_TYPE.Wind, true, showHPBar: true);
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