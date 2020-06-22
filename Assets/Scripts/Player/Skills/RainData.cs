using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Locations.Features;

public class RainData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.RAIN;
    public override string name => "Rain";
    public override string description => "Applies Wet to all tiles and objects that are outside structures on the target area.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;

    public RainData() : base() {
        targetTypes = new[] { SPELL_TARGET.HEX };
    }

    public override void ActivateAbility(HexTile targetHex) {
        targetHex.featureComponent.AddFeature(TileFeatureDB.Rain_Feature, targetHex);
        base.ActivateAbility(targetHex);
    }
    public override bool CanPerformAbilityTowards(HexTile targetHex) {
        bool canPerform = base.CanPerformAbilityTowards(targetHex);
        if (canPerform) {
            return targetHex != null
                   && targetHex.biomeType != BIOMES.DESERT
                   && targetHex.featureComponent.HasFeature(TileFeatureDB.Rain_Feature) == false;
        }
        return false;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(tile.collectionOwner.partOfHextile.hexTileOwner);
    }
}
