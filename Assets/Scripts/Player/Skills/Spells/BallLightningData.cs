using Inner_Maps;

public class BallLightningData : SkillData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.BALL_LIGHTNING;
    public override string name => "Ball Lightning";
    public override string description => "This Spell spawns a floating ball of electricity that will move around randomly for a few hours, dealing Electric damage to everything in its path.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;

    public BallLightningData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        BallLightning ballLightning = new BallLightning();
        ballLightning.SetGridTileLocation(targetTile);
        ballLightning.OnPlacePOI();
        ballLightning.SetIsPlayerSource(true);
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
        TileHighlighter.Instance.PositionHighlight(1, tile);
    }
}