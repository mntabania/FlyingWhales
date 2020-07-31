using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonLustData : MinionPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.DEMON_LUST;
    public override string name => "Demon Lust";
    public override string description => "This Demon is a stealthy non-combatant that can deal Fire damage when discovered. It will occasionally apply a temporary debuff on a random Villager to significantly reduce its Mood.";
    public DemonLustData() {
        className = "Lust";
    }
}
