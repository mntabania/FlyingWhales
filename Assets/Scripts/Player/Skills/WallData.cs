using System.Collections.Generic;
using Inner_Maps;
using Traits;

public class WallData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.WALL;
    public override string name => "Wall";
    public override string description => "Wall";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.SPELL;

    public WallData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }
}
