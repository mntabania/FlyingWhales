using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UtilityScripts;
public class SplashWaterData : SkillData {

    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SPLASH_WATER;
    public override string name => "Splash Water";
    public override string description => "This Spell applies Wet to a small area.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;


    public SplashWaterData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }
    public override void ActivateAbility(LocationGridTile targetTile) {
        int processedTileRange = PlayerSkillManager.Instance.GetTileRangeBonusPerLevel(PLAYER_SKILL_TYPE.SPLASH_WATER);
        List<LocationGridTile> tiles = RuinarchListPool<LocationGridTile>.Claim();
        targetTile.PopulateTilesInRadius(tiles, processedTileRange, includeCenterTile: true, includeTilesInDifferentStructure: true);
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            tile.PerformActionOnTraitables(MakeTraitbleWet);
        }
        UnityEngine.GameObject go = GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Water_Bomb);
        go.transform.localScale = new UnityEngine.Vector3(go.transform.localScale.x * (processedTileRange * 0.7f), go.transform.localScale.y * (processedTileRange * 0.7f), go.transform.localScale.z * (processedTileRange * 0.7f));
        targetTile.tileObjectComponent.genericTileObject.traitContainer.AddTrait(targetTile.tileObjectComponent.genericTileObject, "Surprised Remnant");
        //IncreaseThreatThatSeesTile(targetTile, 10);
        RuinarchListPool<LocationGridTile>.Release(tiles);
        base.ActivateAbility(targetTile);
    }
    private void MakeTraitbleWet(ITraitable traitable) {
        //int duration = TraitManager.Instance.allTraits["Wet"].ticksDuration + PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.SPLASH_WATER);
        int duration = PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.SPLASH_WATER);
        traitable.traitContainer.AddTrait(traitable, "Wet", bypassElementalChance: true, overrideDuration: duration);
        Wet wet = traitable.traitContainer.GetTraitOrStatus<Wet>("Wet");
        wet?.SetIsPlayerSource(true);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason) {
        bool canPerform = base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason);
        if (canPerform) {
            return targetTile.structure != null;
        }
        return canPerform;
    }
    public override void ShowValidHighlight(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(PlayerSkillManager.Instance.GetTileRangeBonusPerLevel(PLAYER_SKILL_TYPE.SPLASH_WATER), tile);
    }
}

