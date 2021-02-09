using System.Collections.Generic;
using Inner_Maps;
using Locations.Tile_Features;
using UnityEngine;

public class PoisonBloomData : SkillData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.POISON_BLOOM;
    public override string name => "Poison Bloom";
    public override string description => "Random spots in the ground will start emitting small Poison Clouds that move around and then dissipates.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;

    public PoisonBloomData() : base() {
        targetTypes = new[] { SPELL_TARGET.HEX };
    }

    public override void ActivateAbility(HexTile targetHex) {
        targetHex.featureComponent.AddFeature(TileFeatureDB.Poison_Bloom_Feature, targetHex);
        base.ActivateAbility(targetHex);
    }
    public override bool CanPerformAbilityTowards(HexTile targetHex) {
        bool canPerform = base.CanPerformAbilityTowards(targetHex);
        if (canPerform) {
            return targetHex != null && targetHex.featureComponent.HasFeature(TileFeatureDB.Poison_Bloom_Feature) == false;
        }
        return canPerform;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(tile.parentArea);
    }
}