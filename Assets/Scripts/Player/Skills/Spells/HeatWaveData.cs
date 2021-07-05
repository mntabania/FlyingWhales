using System.Collections.Generic;
using Inner_Maps;
using Locations.Area_Features;
using UnityEngine;

public class HeatWaveData : SkillData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.HEAT_WAVE;
    public override string name => "Heat Wave";
    public override string description => "This Spell summons a blistering heatwave over a large area. Characters caught outside within the Heatwave may get stacks of Overheating. It does not affect characters inside structures.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

    public HeatWaveData() : base() {
        targetTypes = new[] { SPELL_TARGET.AREA };
    }

    public override void ActivateAbility(Area targetArea) {
        HeatWaveFeature feature = targetArea.featureComponent.AddFeature(AreaFeatureDB.Heat_Wave_Feature, targetArea) as HeatWaveFeature;
        feature.SetIsPlayerSource(true);
        base.ActivateAbility(targetArea);
    }
    public override bool CanPerformAbilityTowards(Area targetArea) {
        bool canPerform = base.CanPerformAbilityTowards(targetArea);
        if (canPerform) {
            return targetArea != null
                   // && targetArea.biomeType != BIOMES.SNOW
                   && targetArea.featureComponent.HasFeature(AreaFeatureDB.Heat_Wave_Feature) == false;
        }
        return canPerform;
    }
    public override void ShowValidHighlight(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(tile.area);
    }
}
