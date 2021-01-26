﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class FeebleSpiritData : SkillData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.FEEBLE_SPIRIT;
    public override string name => "Feeble Spirit";
    public override string description => "This Spell summons a Feeble Spirit that will drain Energy from a nearby Resident.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 1;

    public FeebleSpiritData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        FeebleSpirit spirit = InnerMapManager.Instance.CreateNewTileObject<FeebleSpirit>(TILE_OBJECT_TYPE.FEEBLE_SPIRIT);
        spirit.SetGridTileLocation(targetTile);
        spirit.OnPlacePOI();
        //IncreaseThreatThatSeesTile(targetTile, 10);
        base.ActivateAbility(targetTile);
        // targetTile.structure.AddPOI(spirit, targetTile);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile) {
        bool canPerform = base.CanPerformAbilityTowards(targetTile);
        if (canPerform) {
            return targetTile.structure != null && targetTile.objHere == null;
        }
        return canPerform;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(0, tile);
    }
}