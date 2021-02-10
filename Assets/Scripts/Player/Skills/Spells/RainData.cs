using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Locations.Area_Features;

public class RainData : SkillData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.RAIN;
    public override string name => "Rain";
    public override string description => "This Spell will generate rainfall on the target area, applying Wet to anything outside structures.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;

    public RainData() : base() {
        targetTypes = new[] { SPELL_TARGET.HEX };
    }

    public override void ActivateAbility(HexTile targetHex) {
        targetHex.featureComponent.AddFeature(AreaFeatureDB.Rain_Feature, targetHex);
        base.ActivateAbility(targetHex);
    }
    public override bool CanPerformAbilityTowards(HexTile targetHex) {
        bool canPerform = base.CanPerformAbilityTowards(targetHex);
        if (canPerform) {
            return targetHex != null
                   // && targetHex.biomeType != BIOMES.DESERT
                   && targetHex.featureComponent.HasFeature(AreaFeatureDB.Rain_Feature) == false;
        }
        return false;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(tile.parentArea);
    }
}
