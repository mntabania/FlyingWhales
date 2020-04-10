using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class LazinessData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.LAZINESS;
    public override string name { get { return "Laziness"; } }
    public override string description { get { return "Laziness"; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.AFFLICTION; } }

    public LazinessData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }
}