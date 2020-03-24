using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class BrimstonesData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.BRIMSTONES;
    public override string name { get { return "Brimstones"; } }
    public override string description { get { return "Burning meteorites will strike random tiles on the target area, dealing Fire damage."; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.SPELL; } }
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 1;

    public BrimstonesData() : base() {
        targetTypes = new[]{ SPELL_TARGET.HEX };
    }
    public override void ActivateAbility(HexTile targetHex) {
        targetHex.spellsComponent.SetHasBrimstones(true);
        base.ActivateAbility(targetHex);
    }
    public override bool CanPerformAbilityTowards(HexTile targetHex) {
        return targetHex != null && !targetHex.spellsComponent.hasBrimstones;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(tile.collectionOwner.partOfHextile.hexTileOwner);
    }
}