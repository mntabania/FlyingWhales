using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class RavenousSpiritData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.RAVENOUS_SPIRIT;
    public override string name => "Ravenous Spirit";
    public override string description => "This Spell summons a Ravenous Spirit that will drain Fullness from a nearby Resident.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 1;

    public RavenousSpiritData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        RavenousSpirit spirit = InnerMapManager.Instance.CreateNewTileObject<RavenousSpirit>(TILE_OBJECT_TYPE.RAVENOUS_SPIRIT);
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
