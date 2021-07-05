using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class ForlornSpiritData : SkillData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.FORLORN_SPIRIT;
    public override string name => "Forlorn Spirit";
    public override string description => "This Spell summons a Forlorn Spirit that will drain Happiness from a nearby Villager." +
        "\nA Villager produces a Chaos Orb whenever it becomes Sulking.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 1;

    public ForlornSpiritData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        ForlornSpirit spirit = InnerMapManager.Instance.CreateNewTileObject<ForlornSpirit>(TILE_OBJECT_TYPE.FORLORN_SPIRIT);
        spirit.SetGridTileLocation(targetTile);
        spirit.OnPlacePOI();
        //IncreaseThreatThatSeesTile(targetTile, 10);
        base.ActivateAbility(targetTile);
        // targetTile.structure.AddPOI(spirit, targetTile);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason) {
        bool canPerform = base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason);
        if (canPerform) {
            return targetTile.structure != null && targetTile.tileObjectComponent.objHere == null;
        }
        return canPerform;
    }
    public override void ShowValidHighlight(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(0, tile);
    }
}
