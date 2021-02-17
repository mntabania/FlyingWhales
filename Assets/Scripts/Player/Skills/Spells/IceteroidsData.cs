using Inner_Maps;

public class IceteroidsData : SkillData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.ICETEROIDS;
    public override string name => "Iceteroids";
    public override string description => "This Spell will make dozens of icy rocks come crashing down from space onto a target area, dealing Ice damage to anything they hit.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 1;

    public IceteroidsData() : base() {
        targetTypes = new[]{ SPELL_TARGET.AREA };
    }
    public override void ActivateAbility(Area targetArea) {
        targetArea.spellsComponent.SetHasIceteroids(true);
        base.ActivateAbility(targetArea);
    }
    public override bool CanPerformAbilityTowards(Area targetArea) {
        bool canPerform = base.CanPerformAbilityTowards(targetArea);
        if (canPerform) {
            return targetArea != null && !targetArea.spellsComponent.hasIceteroids;
        }
        return false;
    }
    public override void ShowValidHighlight(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(tile.area);
    }
}