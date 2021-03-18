using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonPrideData : MinionPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DEMON_PRIDE;
    public override string name => "Pride Demon";
    public override string description => "This Lesser Demon is a Magic Combatant that deals Water damage. It can also heal allies.";

    public DemonPrideData() {
        minionType = MINION_TYPE.Pride;
        className = "Pride";
    }
}
