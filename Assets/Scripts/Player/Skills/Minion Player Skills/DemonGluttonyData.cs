using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonGluttonyData : MinionPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.DEMON_GLUTTONY;
    public override string name { get { return "Demon Gluttony"; } }
    public override string description { get { return "Demon Gluttony"; } }

    public DemonGluttonyData() {
        className = "Gluttony";
    }
}
