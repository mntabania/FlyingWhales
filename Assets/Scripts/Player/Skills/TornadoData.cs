using Inner_Maps;
using UnityEngine;
public class TornadoData : SpellData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.TORNADO;
    public override string name => "Tornado";
    public override string description => "This Spell summons a devastating Tornado that moves around randomly. It deals a high amount of Wind damage to everything it comes in contact with.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public TornadoData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        Tornado tornado = new Tornado();
        tornado.SetExpiryDate(GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(Random.Range(1, 4))));
        tornado.SetGridTileLocation(targetTile);
        tornado.OnPlacePOI();
        //IncreaseThreatThatSeesTile(targetTile, 10);
        base.ActivateAbility(targetTile);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile) {
        bool canPerform = base.CanPerformAbilityTowards(targetTile);
        if (canPerform) {
            return targetTile.structure != null;
        }
        return canPerform;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(2, tile);
    }
}
