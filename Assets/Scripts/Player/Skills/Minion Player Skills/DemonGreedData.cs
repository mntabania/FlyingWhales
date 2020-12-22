using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonGreedData : MinionPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DEMON_GREED;
    public override string name { get { return "Greed Demon"; } }
    public override string description => "This Demon is a harsh melee combatant that deals Wind damage. Can be summoned to invade villages in the region it was spawned.";

    public DemonGreedData() {
        className = "Greed";
    }
}
