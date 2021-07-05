using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Locations.Area_Features;

public class RainData : SkillData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.RAIN;
    public override string name => "Rain";
    public override string description => "This Spell will generate rainfall on the target area, applying Wet to anything outside structures and caves.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;

    public RainData() : base() {
        targetTypes = new[] { SPELL_TARGET.AREA };
    }

    public override void ActivateAbility(Area targetArea) {
        AreaFeature feature = targetArea.featureComponent.AddFeature(AreaFeatureDB.Rain_Feature, targetArea);
        base.ActivateAbility(targetArea);
    }
    public override bool CanPerformAbilityTowards(Area targetArea) {
        bool canPerform = base.CanPerformAbilityTowards(targetArea);
        if (canPerform) {
            return targetArea != null
                   // && targetHex.biomeType != BIOMES.DESERT
                   && targetArea.featureComponent.HasFeature(AreaFeatureDB.Rain_Feature) == false;
        }
        return false;
    }
    public override void ShowValidHighlight(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(tile.area);
    }
}
