﻿using System.Collections;
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
        targetTypes = new[] { SPELL_TARGET.HEX };
    }
    public override void ActivateAbility(HexTile targetHex) {
        targetHex.spellsComponent.SetHasElectricStorm(true);
        base.ActivateAbility(targetHex);
    }
    public override bool CanPerformAbilityTowards(HexTile targetHex) {
        bool canPerform = base.CanPerformAbilityTowards(targetHex);
        if (canPerform) {
            return targetHex != null && !targetHex.spellsComponent.hasElectricStorm;
        }
        return canPerform;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(tile.area);
    }
}