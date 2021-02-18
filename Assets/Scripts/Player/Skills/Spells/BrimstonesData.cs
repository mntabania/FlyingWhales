using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class BrimstonesData : SkillData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.BRIMSTONES;
    public override string name => "Brimstones";
    public override string description => "This Spell will make dozens of burning rocks come crashing down from space onto a target area, dealing Fire damage to anything they hit.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 1;

    public BrimstonesData() : base() {
        targetTypes = new[]{ SPELL_TARGET.AREA };
    }
    public override void ActivateAbility(Area targetArea) {
        targetArea.spellsComponent.SetHasBrimstones(true);
        base.ActivateAbility(targetArea);
    }
    public override bool CanPerformAbilityTowards(Area targetArea) {
        bool canPerform = base.CanPerformAbilityTowards(targetArea);
        if (canPerform) {
            return targetArea != null && !targetArea.spellsComponent.hasBrimstones;
        }
        return canPerform;
    }
    public override void ShowValidHighlight(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(tile.area);
    }
}