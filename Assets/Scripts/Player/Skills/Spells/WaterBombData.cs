using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UtilityScripts;
public class WaterBombData : SkillData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.WATER_BOMB;
    public override string name => "Water Bomb";
    public override string description => "Applies Wet to a 2 tile radius around the designated spot.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;

    public WaterBombData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        List<LocationGridTile> tiles = RuinarchListPool<LocationGridTile>.Claim();
        targetTile.PopulateTilesInRadius(tiles, 1, includeCenterTile: true, includeTilesInDifferentStructure: true);
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            tile.PerformActionOnTraitables(MakeTraitbleWet);
        }
        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Water_Bomb);
        targetTile.tileObjectComponent.genericTileObject.traitContainer.AddTrait(targetTile.tileObjectComponent.genericTileObject, "Surprised Remnant");
        //IncreaseThreatThatSeesTile(targetTile, 10);
        RuinarchListPool<LocationGridTile>.Release(tiles);
        base.ActivateAbility(targetTile);
    }
    private void MakeTraitbleWet(ITraitable traitable) {
        traitable.traitContainer.AddTrait(traitable, "Wet");
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

