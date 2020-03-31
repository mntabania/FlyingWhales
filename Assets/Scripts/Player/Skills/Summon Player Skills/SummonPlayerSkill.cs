using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonPlayerSkill : SpellData {
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.SUMMON; } }
    public string className { get; protected set; }
    public RACE race { get; protected set; }

    public SummonPlayerSkill() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }
}