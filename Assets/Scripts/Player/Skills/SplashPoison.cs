using System.Collections.Generic;
using Inner_Maps;
using Scriptable_Object_Scripts;
using Traits;
using UtilityScripts;

public class SplashPoison : PlayerSpell {

    public SplashPoison() : base(SPELL_TYPE.SPLASH_POISON) {
        SetDefaultCooldownTime(24);
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
        tier = 1;
    }
    
}

public class SplashPoisonData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.SPLASH_POISON;
    public override string name => "Splash Poison";
    public override string description => "This Spell applies Poisoned to a 3x3 tile floor.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;

    public SplashPoisonData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        AudioManager.Instance.CreateAudioObject(
            CollectionUtilities.GetRandomElement(PlayerSkillManager.Instance
                .GetPlayerSkillAsset<SplashPoisonAssets>(SPELL_TYPE.SPLASH_POISON).splashSounds),
            targetTile, 3, false
        );
        List<LocationGridTile> tiles = targetTile.GetTilesInRadius(1, includeCenterTile: true, includeTilesInDifferentStructure: true);
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            tile.PerformActionOnTraitables(MakeTraitblePoisoned);
        }
        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Poison_Bomb);
        targetTile.genericTileObject.traitContainer.AddTrait(targetTile.genericTileObject, "Surprised Remnant");
        //IncreaseThreatThatSeesTile(targetTile, 10);
        base.ActivateAbility(targetTile);
    }
    private void MakeTraitblePoisoned(ITraitable traitable) {
        traitable.traitContainer.AddTrait(traitable, "Poisoned", bypassElementalChance: true);
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

