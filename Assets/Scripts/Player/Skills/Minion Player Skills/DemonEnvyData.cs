using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonEnvyData : MinionPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DEMON_ENVY;
    public override string name => "Envy Demon";
    public override string description => "This Demon is a non-combatant with a special utility. When spawned, it will immediately cast a spell that will ensnare all nearby Villagers and prevent them from moving for a duration. It will immediately despawn afterwards.";

    public DemonEnvyData() {
        className = "Envy";
    }
}
