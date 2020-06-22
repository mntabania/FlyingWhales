using System.Collections.Generic;
using Inner_Maps;
using Scriptable_Object_Scripts;
using Traits;
using UtilityScripts;

public class WindBlastData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.WIND_BLAST;
    public override string name => "Wind Blast";
    public override string description => "This Spell blasts a powerful wave of air outward from a target spot, dealing Wind damage to anything it hits.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.SPELL;

    public WindBlastData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        AudioManager.Instance.CreateAudioObject(
            PlayerSkillManager.Instance.GetPlayerSkillData<WindBlastSkillData>(SPELL_TYPE.WIND_BLAST).blastSound,
            targetTile, 3, false
        );
        
        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Wind_Blast);
        List<LocationGridTile> tiles = targetTile.GetTilesInRadius(1, includeCenterTile: true, includeTilesInDifferentStructure: true);
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            tile.PerformActionOnTraitables(ApplyWindDamage);
        }
        targetTile.genericTileObject.traitContainer.AddTrait(targetTile.genericTileObject, "Danger Remnant");
        //IncreaseThreatThatSeesTile(targetTile, 10);
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