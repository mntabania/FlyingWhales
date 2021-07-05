using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonLustData : MinionPlayerSkill {
    
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DEMON_LUST;
    public override string name => "Lust Demon";
    public override string description => "This Lesser Demon is a Magic Combatant that attacks using Fire. It has has high damage but low HP.";

    public DemonLustData() {
        minionType = MINION_TYPE.Lust;
        className = "Lust";
    }
}
