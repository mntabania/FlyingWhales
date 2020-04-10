using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonWrathData : MinionPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.DEMON_WRATH;
    public override string name { get { return "Demon Wrath"; } }
    public override string description { get { return "Demon Wrath"; } }

    public DemonWrathData() {
        className = "Wrath";
    }
}
