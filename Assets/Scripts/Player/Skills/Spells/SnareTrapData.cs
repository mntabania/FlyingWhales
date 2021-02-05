﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class SnareTrapData : SkillData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SNARE_TRAP;
    public override string name => "Snare Trap";
    public override string description => "This Spell places an invisible trap on a target unoccupied tile. Any character that walks into the tile will activate it and become Ensnared.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;
    public virtual int abilityRadius => 1;

    public SnareTrapData() : base() {
        targetTypes = new[] { SPELL_TARGET.TILE };
    }
    public override void ActivateAbility(LocationGridTile targetTile) {
        targetTile.SetHasSnareTrap(true);
        //IncreaseThreatThatSeesTile(targetTile, 10);
        base.ActivateAbility(targetTile);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile) {
        bool canPerform = base.CanPerformAbilityTowards(targetTile);
        if (canPerform) {
            return !targetTile.hasSnareTrap;
        }
        return false;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(0, tile);
    }
}