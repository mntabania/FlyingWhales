using Inner_Maps;

public class IceteroidsData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.ICETEROIDS;
    public override string name => "Iceteroids";
    public override string description => "Frozen meteorites will strike random tiles on the target area, dealing Ice damage.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 1;

    public IceteroidsData() : base() {
        targetTypes = new[]{ SPELL_TARGET.HEX };
    }
    public override void ActivateAbility(HexTile targetHex) {
        targetHex.spellsComponent.SetHasIceteroids(true);
        base.ActivateAbility(targetHex);
    }
    public override bool CanPerformAbilityTowards(HexTile targetHex) {
        bool canPerform = base.CanPerformAbilityTowards(targetHex);
        if (canPerform) {
            return targetHex != null && !targetHex.spellsComponent.hasIceteroids;
        }
        return canPerform;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(tile.collectionOwner.partOfHextile.hexTileOwner);
    }
}