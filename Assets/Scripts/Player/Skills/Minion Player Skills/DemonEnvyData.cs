using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonEnvyData : MinionPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.DEMON_ENVY;
    public override string name { get { return "Demon Envy"; } }
    public override string description { get { return "Demon Envy"; } }

    public DemonEnvyData() {
        className = "Envy";
    }
}
