using System.Collections.Generic;
using Inner_Maps;
using Locations.Area_Features;
using UnityEngine;

public class BlizzardData : SkillData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.BLIZZARD;
    public override string name => "Blizzard";
    public override string description => "This Spell summons a chilling Blizzard over a large area. Characters caught outside within the Blizzard may get stacks of Freezing, eventually causing them to be Frozen in place. It does not affect characters inside structures. Blizzard cannot be cast on a desert area.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;

    public BlizzardData() : base() {
        targetTypes = new[] { SPELL_TARGET.AREA };
    }

    public override void ActivateAbility(Area targetArea) {
        targetArea.featureComponent.AddFeature(AreaFeatureDB.Blizzard_Feature, targetArea);
        base.ActivateAbility(targetArea);
    }
    public override bool CanPerformAbilityTowards(Area targetArea) {
        bool canPerform = base.CanPerformAbilityTowards(targetArea);
        if (canPerform) {
            return targetArea != null
                   && targetArea.biomeType != BIOMES.DESERT
                   && targetArea.featureComponent.HasFeature(AreaFeatureDB.Blizzard_Feature) == false;
        }
        return canPerform;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(tile.area);
    }
}
