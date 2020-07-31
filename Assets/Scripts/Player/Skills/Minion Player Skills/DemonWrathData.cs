using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonWrathData : MinionPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.DEMON_WRATH;
    public override string name => "Demon Wrath";
    public override string description => "This Demon is a powerful melee combatant that deals Normal damage. Can be summoned to invade villages in the region it was spawned.";
    public DemonWrathData() {
        className = "Wrath";
    }
}
