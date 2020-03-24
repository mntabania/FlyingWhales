using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class PoisonBloomData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.POISON_BLOOM;
    public override string name => "Poison Bloom";
    public override string description => "Random spots in the ground will start emitting small Poison Clouds that move around and then dissipates.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.DEVASTATION;
    public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;

    public PoisonBloomData() : base() {
        targetTypes = new[] { SPELL_TARGET.HEX };
    }

    public override void ActivateAbility(HexTile targetHex) {
        targetHex.featureComponent.AddFeature(TileFeatureDB.Poison_Bloom_Feature, targetHex);
        base.ActivateAbility(targetHex);
    }
    public override bool CanPerformAbilityTowards(HexTile targetHex) {
        return targetHex != null && targetHex.featureComponent.HasFeature(TileFeatureDB.Poison_Bloom_Feature) == false;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(tile.collectionOwner.partOfHextile.hexTileOwner);
    }
}