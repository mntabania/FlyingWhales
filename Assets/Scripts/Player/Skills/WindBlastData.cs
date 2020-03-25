using System.Collections.Generic;
using Inner_Maps;
using Traits;

public class WindBlastData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.WIND_BLAST;
    public override string name => "Wind Blast";
    public override string description => "Pushes movable characters and objects outwards and applies a moderate amount of Wind damage.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.SPELL;

    public WindBlastData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Wind_Blast);
        List<LocationGridTile> tiles = targetTile.GetTilesInRadius(1, includeCenterTile: true, includeTilesInDifferentStructure: false);
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            tile.PerformActionOnTraitables(ApplyWindDamage);
        }
    }
    private void ApplyWindDamage(ITraitable traitable) {
        traitable.AdjustHP(-30, ELEMENTAL_TYPE.Wind, true, showHPBar: true);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile) {
        return targetTile.structure != null;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(1, tile);
    }
}