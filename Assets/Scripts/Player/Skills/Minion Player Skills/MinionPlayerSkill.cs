using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionPlayerSkill : SpellData {
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.MINION; } }
    public string className { get; protected set; }

    public MinionPlayerSkill() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }
}
