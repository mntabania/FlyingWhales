using System.Collections.Generic;
using Inner_Maps;
using Locations.Area_Features;
using UnityEngine;

public class PoisonBloomData : SkillData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.POISON_BLOOM;
    public override string name => "Poison Bloom";
    public override string description => "Random spots in the ground will start emitting small Poison Clouds that move around and then dissipates.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;

    public PoisonBloomData() : base() {
        targetTypes = new[] { SPELL_TARGET.AREA };
    }

    public override void ActivateAbility(Area targetArea) {
        AreaFeature feature = targetArea.featureComponent.AddFeature(AreaFeatureDB.Poison_Bloom_Feature, targetArea);
        (feature as PoisonBloomFeature).SetIsPlayerSource(true);
        base.ActivateAbility(targetArea);
    }
    public override bool CanPerformAbilityTowards(Area targetArea) {
        bool canPerform = base.CanPerformAbilityTowards(targetArea);
        if (canPerform) {
            return targetArea != null && targetArea.featureComponent.HasFeature(AreaFeatureDB.Poison_Bloom_Feature) == false;
        }
        return canPerform;
    }
    public override void ShowValidHighlight(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(tile.area);
    }
}