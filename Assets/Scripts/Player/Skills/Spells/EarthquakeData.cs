using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class EarthquakeData : SkillData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.EARTHQUAKE;
    public override string name => "Earthquake";
    public override string description => "This Spell will cause the ground to shake vigorously, dealing a small amount of Earth damage to everyone in range. Objects may get moved around.";
    public override PLAYER_SKILL_CATEGORY category { get { return PLAYER_SKILL_CATEGORY.SPELL; } }
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 1;

    public EarthquakeData() : base() {
        targetTypes = new[] { SPELL_TARGET.AREA };
    }

    public override void ActivateAbility(Area targetArea) {
        targetArea.spellsComponent.SetHasEarthquake(true);
        base.ActivateAbility(targetArea);
    }
    public override bool CanPerformAbilityTowards(Area targetArea) {
        bool canPerform = base.CanPerformAbilityTowards(targetArea);
        if (canPerform) {
            return targetArea != null && !targetArea.spellsComponent.hasEarthquake;
        }
        return canPerform;
    }
    public override void ShowValidHighlight(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(tile.area);
    }
}