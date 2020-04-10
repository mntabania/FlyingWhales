using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonLustData : MinionPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.DEMON_LUST;
    public override string name { get { return "Demon Lust"; } }
    public override string description { get { return "Demon Lust"; } }

    public DemonLustData() {
        className = "Lust";
    }
}
