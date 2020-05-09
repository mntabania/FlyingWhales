using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheProfaneData : DemonicStructurePlayerSkill {
    public override string name => "The Profane";
    public override SPELL_TYPE type => SPELL_TYPE.THE_PROFANE;

    public TheProfaneData() {
        structureType = STRUCTURE_TYPE.THE_PROFANE;
    }
}
