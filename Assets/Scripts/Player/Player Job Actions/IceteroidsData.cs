using Inner_Maps;

public class IceteroidsData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.ICETEROIDS;
    public override string name => "Iceteroids";
    public override string description => "Small icy rocks fall down from the sky and randomly hits spots within the area. Any object hit is dealt Ice damage.";
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
        return targetHex != null && !targetHex.spellsComponent.hasIceteroids;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(tile.collectionOwner.partOfHextile.hexTileOwner);
    }
}