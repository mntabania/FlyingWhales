using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonLustData : MinionPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.DEMON_LUST;
    public override string name => "Lust Demon";
    public override string description => "This Demon is a non-combatant with a special utility. When spawned, it will immediately cast a spell that will apply a temporary debuff on all nearby Villagers to significantly reduce their Mood. It will immediately despawn afterwards.";

    public DemonLustData() {
        className = "Lust";
    }
}
