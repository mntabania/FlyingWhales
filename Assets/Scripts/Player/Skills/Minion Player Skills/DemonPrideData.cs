using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonPrideData : MinionPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.DEMON_PRIDE;
    public override string name => "Demon Pride";
    public override string description => "This Demon is a strong ranged magic-user that deals Electric damage. Can be summoned to invade villages in the region it was spawned.";
    public DemonPrideData() {
        className = "Pride";
    }
}
