﻿using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class LocustSwarm : PlayerSpell {

    public LocustSwarm() : base(SPELL_TYPE.LOCUST_SWARM) {
        SetDefaultCooldownTime(24);
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
        tier = 1;
    }

    #region Overrides
    public override void ActivateAction(LocationGridTile targetTile) {
        base.ActivateAction(targetTile);
        LocustSwarmTileObject tornadoTileObject = new LocustSwarmTileObject();
        tornadoTileObject.SetGridTileLocation(targetTile);
        tornadoTileObject.OnPlacePOI();
    }
    public virtual bool CanTarget(LocationGridTile tile) {
        return tile.structure != null;
    }
    protected virtual bool CanPerformActionTowards(LocationGridTile tile) {
        return tile.structure != null;
    }
    #endregion
}

public class LocustSwarmData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.LOCUST_SWARM;
    public override string name => "Locust Swarm";
    public override string description => "This Spell spawns a swarm of hungry locusts that would roam around randomly for a few hours, eating everything edible in its path.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 2;

    public LocustSwarmData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        LocustSwarmTileObject tornadoTileObject = new LocustSwarmTileObject();
        tornadoTileObject.SetGridTileLocation(targetTile);
        tornadoTileObject.OnPlacePOI();
        //IncreaseThreatThatSeesTile(targetTile, 10);
        base.ActivateAbility(targetTile);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile) {
        bool canPerform = base.CanPerformAbilityTowards(targetTile);
        if (canPerform) {
            return targetTile.structure != null;
        }
        return canPerform;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(2, tile);
    }
}
