using System.Collections.Generic;
using Inner_Maps;
using Traits;

public class SplashPoison : PlayerSpell {

    public SplashPoison() : base(SPELL_TYPE.SPLASH_POISON) {
        SetDefaultCooldownTime(24);
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
        tier = 1;
    }
    
}

public class SplashPoisonData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.SPLASH_POISON;
    public override string name => "Splash Poison";
    public override string description => "Applies Poisoned to a 2 tile radius around the designated spot.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.DEVASTATION;
    public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;

    public SplashPoisonData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        List<LocationGridTile> tiles = targetTile.GetTilesInRadius(1, includeCenterTile: true, includeTilesInDifferentStructure: true);
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            tile.PerformActionOnTraitables(MakeTraitblePoisoned);
        }
        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Poison_Bomb);
    }
    private void MakeTraitblePoisoned(ITraitable traitable) {
        traitable.traitContainer.AddTrait(traitable, "Poisoned");
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile) {
        return targetTile.structure != null;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(1, tile);
    }
}

