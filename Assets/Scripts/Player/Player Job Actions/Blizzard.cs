using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class Blizzard : PlayerSpell {

    public Blizzard() : base(SPELL_TYPE.BLIZZARD) {
        SetDefaultCooldownTime(24);
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
        tier = 1;
    }
}

public class BlizzardData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.BLIZZARD;
    public override string name { get { return "Blizzard"; } }
    public override string description { get { return "Significantly lowers the outside temperature on the target area, which may randomly apply Freezing on affected characters."; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.DEVASTATION; } }
    public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public override int abilityRadius => 1;

    public BlizzardData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        targetTile.buildSpotOwner.hexTileOwner.featureComponent.AddFeature(TileFeatureDB.Blizzard_Feature, targetTile.buildSpotOwner.hexTileOwner);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile) {
        return targetTile.buildSpotOwner.isPartOfParentRegionMap 
               && targetTile.buildSpotOwner.hexTileOwner.biomeType != BIOMES.DESERT
               && targetTile.buildSpotOwner.hexTileOwner.featureComponent.HasFeature(TileFeatureDB.Blizzard_Feature) == false;
    }
    public override void ShowRange(LocationGridTile targetTile) {
        base.ShowRange(targetTile);
        List<LocationGridTile> tiles = targetTile.GetTilesInRadius(abilityRadius, 0, true);
        InnerMapManager.Instance.HighlightTiles(tiles);
    }
    public override void HideRange(LocationGridTile targetTile) {
        base.HideRange(targetTile);
        List<LocationGridTile> tiles = targetTile.GetTilesInRadius(abilityRadius, 0, true);
        InnerMapManager.Instance.UnhighlightTiles(tiles);
    }
}
