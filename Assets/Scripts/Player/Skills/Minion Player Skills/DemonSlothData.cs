using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonSlothData : MinionPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.DEMON_SLOTH;
    public override string name { get { return "Demon Sloth"; } }
    public override string description { get { return "Demon Sloth"; } }

    public DemonSlothData() {
        className = "Sloth";
    }
}
