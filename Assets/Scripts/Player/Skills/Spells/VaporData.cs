using Inner_Maps;

public class VaporData : SkillData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.VAPOR;
    public override string name => "Vapor";
    public override string description => "Vapor";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;

    public VaporData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        Vapor vapor = new Vapor();
        vapor.SetGridTileLocation(targetTile);
        vapor.OnPlacePOI();
        vapor.SetStacks(EditableValuesManager.Instance.vaporStacks);
        vapor.SetIsPlayerSource(true);
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