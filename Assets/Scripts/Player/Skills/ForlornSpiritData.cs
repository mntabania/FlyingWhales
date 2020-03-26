using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class ForlornSpiritData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.FORLORN_SPIRIT;
    public override string name { get { return "Forlorn Spirit"; } }
    public override string description { get { return "Roams around and then drains Entertainment of the first character that gets in range. Dissipates after an hour."; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.SPELL; } }
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 1;

    public ForlornSpiritData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        ForlornSpirit spirit = InnerMapManager.Instance.CreateNewTileObject<ForlornSpirit>(TILE_OBJECT_TYPE.FORLORN_SPIRIT);
        spirit.SetGridTileLocation(targetTile);
        spirit.OnPlacePOI();
        IncreaseThreatThatSeesTile(targetTile, 10);
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
