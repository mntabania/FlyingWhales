using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class EarthquakeData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.EARTHQUAKE;
    public override string name { get { return "Earthquake"; } }
    public override string description { get { return "Violently shakes the ground, dealing Normal damage to objects and randomly moving them around. Characters will become Disoriented."; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.DEVASTATION; } }
    public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 1;

    public EarthquakeData() : base() {
        targetTypes = new[] { SPELL_TARGET.HEX };
    }

    public override void ActivateAbility(HexTile targetHex) {
        targetHex.spellsComponent.SetHasEarthquake(true);
    }
    public override bool CanPerformAbilityTowards(HexTile targetHex) {
        return targetHex != null && !targetHex.spellsComponent.hasEarthquake;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(tile.buildSpotOwner.hexTileOwner);
    }
}