using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonGreedData : MinionPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DEMON_GREED;
    public override string name { get { return "Greed Demon"; } }
    public override string description => "This Lesser Demon is a melee Physical Combatant that deals Wind damage. It deals bonus damage when attacking objects and structures.";

    public DemonGreedData() {
        minionType = MINION_TYPE.Greed;
        className = "Greed";
    }
}
