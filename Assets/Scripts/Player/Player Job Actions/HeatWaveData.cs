using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class HeatWaveData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.HEAT_WAVE;
    public override string name => "Heat Wave";
    public override string description => "Significantly increase the outside temperature on the target area, which may randomly apply Overheating on affected characters.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.SPELL;

    public HeatWaveData() : base() {
        targetTypes = new[] { SPELL_TARGET.HEX };
    }

    public override void ActivateAbility(HexTile targetHex) {
        targetHex.featureComponent.AddFeature(TileFeatureDB.Heat_Wave_Feature, targetHex);
        base.ActivateAbility(targetHex);
    }
    public override bool CanPerformAbilityTowards(HexTile targetHex) {
        return targetHex != null
               && targetHex.biomeType != BIOMES.SNOW
               && targetHex.biomeType != BIOMES.FOREST
               && targetHex.featureComponent.HasFeature(TileFeatureDB.Heat_Wave_Feature) == false;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(tile.collectionOwner.partOfHextile.hexTileOwner);
    }
}
