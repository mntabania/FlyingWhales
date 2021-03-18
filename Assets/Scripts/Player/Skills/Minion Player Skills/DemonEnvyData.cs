using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonEnvyData : MinionPlayerSkill {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DEMON_ENVY;
    public override string name => "Envy Demon";
    public override string description => "This Lesser Demon is a Melee Physical Combatant with Poison attacks. It is effective in knocking out opponents.";

    public DemonEnvyData() {
        minionType = MINION_TYPE.Envy;
        className = "Envy";
    }
}
