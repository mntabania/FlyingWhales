using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonPrideData : MinionPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DEMON_PRIDE;
    public override string name => "Pride Demon";
    public override string description => "This Demon is a strong ranged magic-user that deals Electric damage. Can be summoned to invade villages in the region it was spawned.";

    public DemonPrideData() {
        minionType = MINION_TYPE.Pride;
        className = "Pride";
    }
}
