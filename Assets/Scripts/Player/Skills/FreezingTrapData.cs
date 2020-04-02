using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class FreezingTrapData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.FREEZING_TRAP;
    public override string name { get { return "Freezing Trap"; } }
    public override string description { get { return "Placed on an empty tile. Applies Frozen to the first character that enters the tile, freezing them in place for several hours."; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.SPELL; } }
    public virtual int abilityRadius => 1;

    public FreezingTrapData() : base() {
        targetTypes = new[] { SPELL_TARGET.TILE };
    }
    public override void ActivateAbility(LocationGridTile targetTile) {
        targetTile.SetHasFreezingTrap(true);
        IncreaseThreatThatSeesTile(targetTile, 10);
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