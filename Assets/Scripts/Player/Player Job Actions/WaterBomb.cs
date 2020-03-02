﻿using System.Collections.Generic;
using Inner_Maps;
using Traits;

public class WaterBomb : PlayerSpell {

    public WaterBomb() : base(SPELL_TYPE.WATER_BOMB) {
        SetDefaultCooldownTime(24);
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
        tier = 1;
    }
    
}

public class WaterBombData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.WATER_BOMB;
    public override string name => "Water Bomb";
    public override string description => "Applies Wet to a 2 tile radius around the designated spot.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.DEVASTATION;
    public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;

    public WaterBombData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        List<LocationGridTile> tiles = targetTile.GetTilesInRadius(1, includeCenterTile: true, includeTilesInDifferentStructure: true);
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            tile.PerformActionOnTraitables(MakeTraitbleWet);
        }
        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Water_Bomb);
    }
    private void MakeTraitbleWet(ITraitable traitable) {
        traitable.traitContainer.AddTrait(traitable, "Wet");
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile) {
        return targetTile.structure != null;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(1, tile);
    }
}

