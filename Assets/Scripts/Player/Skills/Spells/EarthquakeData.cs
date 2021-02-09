﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class EarthquakeData : SkillData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.EARTHQUAKE;
    public override string name { get { return "Earthquake"; } }
    public override string description { get { return "Violently shakes the ground, dealing Normal damage to objects and randomly moving them around. Characters will become Disoriented."; } }
    public override PLAYER_SKILL_CATEGORY category { get { return PLAYER_SKILL_CATEGORY.SPELL; } }
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 1;

    public EarthquakeData() : base() {
        targetTypes = new[] { SPELL_TARGET.HEX };
    }

    public override void ActivateAbility(HexTile targetHex) {
        targetHex.spellsComponent.SetHasEarthquake(true);
        base.ActivateAbility(targetHex);
    }
    public override bool CanPerformAbilityTowards(HexTile targetHex) {
        bool canPerform = base.CanPerformAbilityTowards(targetHex);
        if (canPerform) {
            return targetHex != null && !targetHex.spellsComponent.hasEarthquake;
        }
        return canPerform;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(tile.parentArea);
    }
}