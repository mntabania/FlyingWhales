﻿using Inner_Maps;

public class IceteroidsData : SkillData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.ICETEROIDS;
    public override string name => "Iceteroids";
    public override string description => "This Spell will make dozens of icy rocks come crashing down from space onto a target area, dealing Ice damage to anything they hit.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 1;

    public IceteroidsData() : base() {
        targetTypes = new[]{ SPELL_TARGET.HEX };
    }
    public override void ActivateAbility(HexTile targetHex) {
        targetHex.spellsComponent.SetHasIceteroids(true);
        base.ActivateAbility(targetHex);
    }
    public override bool CanPerformAbilityTowards(HexTile targetHex) {
        bool canPerform = base.CanPerformAbilityTowards(targetHex);
        if (canPerform) {
            return targetHex != null && !targetHex.spellsComponent.hasIceteroids;
        }
        return false;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(tile.parentArea);
    }
}