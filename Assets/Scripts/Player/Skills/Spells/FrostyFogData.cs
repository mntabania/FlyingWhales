using Inner_Maps;

public class FrostyFogData : SkillData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.FROSTY_FOG;
    public override string name { get { return "Frosty Fog"; } }
    public override string description { get { return "Frosty Fog"; } }
    public override PLAYER_SKILL_CATEGORY category { get { return PLAYER_SKILL_CATEGORY.SPELL; } }
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 1;

    public FrostyFogData() : base() {
        targetTypes = new[] { SPELL_TARGET.TILE };
    }
    public override void ActivateAbility(LocationGridTile targetTile) {
        FrostyFog frostyFog = new FrostyFog();
        frostyFog.SetGridTileLocation(targetTile);
        frostyFog.OnPlacePOI();
        frostyFog.SetStacks(EditableValuesManager.Instance.frostyFogStacks);
        frostyFog.SetIsPlayerSource(true);
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