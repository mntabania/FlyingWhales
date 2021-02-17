using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class ElectricStormData : SkillData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.ELECTRIC_STORM;
    public override string name => "Electric Storm";
    public override string description => "This Spell will spawn a series of lightning strikes onto a target area, dealing Electric damage to anything they hit.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 1;

    public ElectricStormData() : base() {
        targetTypes = new[] { SPELL_TARGET.AREA };
    }
    public override void ActivateAbility(Area targetArea) {
        targetArea.spellsComponent.SetHasElectricStorm(true);
        base.ActivateAbility(targetArea);
    }
    public override bool CanPerformAbilityTowards(Area targetArea) {
        bool canPerform = base.CanPerformAbilityTowards(targetArea);
        if (canPerform) {
            return targetArea != null && !targetArea.spellsComponent.hasElectricStorm;
        }
        return canPerform;
    }
    public override void ShowValidHighlight(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(tile.area);
    }
}