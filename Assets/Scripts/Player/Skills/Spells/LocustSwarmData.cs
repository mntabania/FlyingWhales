using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
public class LocustSwarmData : SkillData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.LOCUST_SWARM;
    public override string name => "Locust Swarm";
    public override string description => "This Spell spawns a swarm of hungry locusts that would roam around randomly for a few hours, eating everything edible in its path.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 2;

    public LocustSwarmData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        LocustSwarm tornado = new LocustSwarm();
        tornado.SetGridTileLocation(targetTile);
        tornado.OnPlacePOI();
        //IncreaseThreatThatSeesTile(targetTile, 10);
        base.ActivateAbility(targetTile);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason) {
        bool canPerform = base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason);
        if (canPerform) {
            return targetTile.structure != null;
        }
        return canPerform;
    }
    public override void ShowValidHighlight(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(2, tile);
    }
}
