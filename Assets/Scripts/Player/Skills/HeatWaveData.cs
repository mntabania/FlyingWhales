﻿using System.Collections.Generic;
using Inner_Maps;
using Locations.Features;
using UnityEngine;

public class HeatWaveData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.HEAT_WAVE;
    public override string name => "Heat Wave";
    public override string description => "This Spell summons a blistering heatwave over a large area. Characters caught outside within the Heatwave may get stacks of Overheating. It does not affect characters inside structures. Heatwave cannot be cast on a snow area.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.SPELL;

    public HeatWaveData() : base() {
        targetTypes = new[] { SPELL_TARGET.HEX };
    }

    public override void ActivateAbility(HexTile targetHex) {
        targetHex.featureComponent.AddFeature(TileFeatureDB.Heat_Wave_Feature, targetHex);
        base.ActivateAbility(targetHex);
    }
    public override bool CanPerformAbilityTowards(HexTile targetHex) {
        bool canPerform = base.CanPerformAbilityTowards(targetHex);
        if (canPerform) {
            return targetHex != null
                   && targetHex.biomeType != BIOMES.SNOW
                   && targetHex.biomeType != BIOMES.FOREST
                   && targetHex.featureComponent.HasFeature(TileFeatureDB.Heat_Wave_Feature) == false;
        }
        return canPerform;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(tile.collectionOwner.partOfHextile.hexTileOwner);
    }
}
