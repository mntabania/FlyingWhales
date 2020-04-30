using System.Collections.Generic;
using Inner_Maps;
using Traits;

public class SplashWaterData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.SPLASH_WATER;
    public override string name => "Splash Water";
    public override string description => "This Spell applies Wet to a 3x3 tile floor.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.SPELL;

    public SplashWaterData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }
}

