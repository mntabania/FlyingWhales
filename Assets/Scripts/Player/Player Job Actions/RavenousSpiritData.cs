using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class RavenousSpiritData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.RAVENOUS_SPIRIT;
    public override string name { get { return "Ravenous Spirit"; } }
    public override string description { get { return "Roams around and then drains Fullness of the first character that gets in range. Dissipates after an hour."; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.SABOTAGE; } }
    public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 1;

    public RavenousSpiritData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        RavenousSpirit spirit = InnerMapManager.Instance.CreateNewTileObject<RavenousSpirit>(TILE_OBJECT_TYPE.RAVENOUS_SPIRIT);
        spirit.SetGridTileLocation(targetTile);
        spirit.OnPlacePOI();
        // targetTile.structure.AddPOI(spirit, targetTile);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile) {
        return targetTile.structure != null && targetTile.objHere == null;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(0, tile);
    }
}
