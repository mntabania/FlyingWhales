﻿using System.Collections.Generic;
using Inner_Maps;
using Locations.Tile_Features;
using UnityEngine;

public class BlizzardData : SkillData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.BLIZZARD;
    public override string name => "Blizzard";
    public override string description => "This Spell summons a chilling Blizzard over a large area. Characters caught outside within the Blizzard may get stacks of Freezing, eventually causing them to be Frozen in place. It does not affect characters inside structures. Blizzard cannot be cast on a desert area.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;

    public BlizzardData() : base() {
        targetTypes = new[] { SPELL_TARGET.HEX };
    }

    public override void ActivateAbility(HexTile targetHex) {
        targetHex.featureComponent.AddFeature(TileFeatureDB.Blizzard_Feature, targetHex);
        base.ActivateAbility(targetHex);
    }
    public override bool CanPerformAbilityTowards(HexTile targetHex) {
        bool canPerform = base.CanPerformAbilityTowards(targetHex);
        if (canPerform) {
            return targetHex != null
                   && targetHex.biomeType != BIOMES.DESERT
                   && targetHex.featureComponent.HasFeature(TileFeatureDB.Blizzard_Feature) == false;
        }
        return canPerform;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(tile.collectionOwner.partOfHextile.hexTileOwner);
    }
}