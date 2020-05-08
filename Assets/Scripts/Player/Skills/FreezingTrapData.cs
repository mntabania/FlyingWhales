using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class FreezingTrapData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.FREEZING_TRAP;
    public override string name { get { return "Freezing Trap"; } }
    public override string description { get { return "This Spell places an invisible trap on a target unoccupied tile. Any character that walks into the tile will activate it and become Frozen."; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.SPELL; } }
    public virtual int abilityRadius => 1;

    public FreezingTrapData() : base() {
        targetTypes = new[] { SPELL_TARGET.TILE };
    }
    public override void ActivateAbility(LocationGridTile targetTile) {
        targetTile.SetHasFreezingTrap(true);
        //IncreaseThreatThatSeesTile(targetTile, 10);
        base.ActivateAbility(targetTile);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile) {
        bool canPerform = base.CanPerformAbilityTowards(targetTile);
        if (canPerform) {
            return !targetTile.hasFreezingTrap;
        }
        return false;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(0, tile);
    }
}