using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class RavenousSpiritData : SkillData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.RAVENOUS_SPIRIT;
    public override string name => "Ravenous Spirit";
    public override string description => "This Spell summons a Ravenous Spirit that will drain Fullness from a nearby Villager." +
        "\nA Villager produces a Chaos Orb whenever it becomes Starving.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;
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
