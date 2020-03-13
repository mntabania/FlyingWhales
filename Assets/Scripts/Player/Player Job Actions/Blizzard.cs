using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class Blizzard : PlayerSpell {

    public Blizzard() : base(SPELL_TYPE.BLIZZARD) {
        SetDefaultCooldownTime(24);
        targetTypes = new[] { SPELL_TARGET.HEX };
        tier = 1;
    }
}

public class BlizzardData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.BLIZZARD;
    public override string name => "Blizzard";
    public override string description => "Significantly lowers the outside temperature on the target area, which may randomly apply Freezing on affected characters.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.DEVASTATION;
    public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;

    public BlizzardData() : base() {
        targetTypes = new[] { SPELL_TARGET.HEX };
    }

    public override void ActivateAbility(HexTile targetHex) {
        targetHex.featureComponent.AddFeature(TileFeatureDB.Blizzard_Feature, targetHex);
    }
    public override bool CanPerformAbilityTowards(HexTile targetHex) {
        return targetHex != null
               && targetHex.biomeType != BIOMES.DESERT
               && targetHex.featureComponent.HasFeature(TileFeatureDB.Blizzard_Feature) == false;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(tile.collectionOwner.partOfHextile.hexTileOwner);
    }
}
