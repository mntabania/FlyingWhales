using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonPrideData : MinionPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.DEMON_PRIDE;
    public override string name { get { return "Demon Pride"; } }
    public override string description { get { return "Demon Pride"; } }

    public DemonPrideData() {
        className = "Pride";
    }
}
