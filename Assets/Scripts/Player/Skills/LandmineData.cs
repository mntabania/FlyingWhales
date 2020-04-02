using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class LandmineData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.LANDMINE;
    public override string name { get { return "Landmine"; } }
    public override string description { get { return "A hidden demonic landmine that deals major Fire damage when a character walks over it."; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.SPELL; } }
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 1;

    public LandmineData() : base() {
        targetTypes = new[] { SPELL_TARGET.TILE };
    }
    public override void ActivateAbility(LocationGridTile targetTile) {
        targetTile.SetHasLandmine(true);
        IncreaseThreatThatSeesTile(targetTile, 10);
        base.ActivateAbility(targetTile);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile) {
        bool canPerform = base.CanPerformAbilityTowards(targetTile);
        if (canPerform) {
            return !targetTile.hasLandmine;
        }
        return canPerform;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(0, tile);
    }
}