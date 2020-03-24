using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class ElectricStormData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.ELECTRIC_STORM;
    public override string name { get { return "Electric Storm"; } }
    public override string description { get { return "Electric Storm"; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.DEVASTATION; } }
    public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 1;

    public ElectricStormData() : base() {
        targetTypes = new[] { SPELL_TARGET.HEX };
    }
    public override void ActivateAbility(HexTile targetHex) {
        targetHex.spellsComponent.SetHasElectricStorm(true);
        base.ActivateAbility(targetHex);
    }
    public override bool CanPerformAbilityTowards(HexTile targetHex) {
        return targetHex != null && !targetHex.spellsComponent.hasElectricStorm;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(tile.collectionOwner.partOfHextile.hexTileOwner);
    }
}