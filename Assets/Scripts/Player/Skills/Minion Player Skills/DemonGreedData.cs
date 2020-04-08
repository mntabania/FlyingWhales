using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonGreedData : MinionPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.DEMON_GREED;
    public override string name { get { return "Demon Greed"; } }
    public override string description { get { return "Demon Greed"; } }

    public DemonGreedData() {
        className = "Greed";
    }
}
