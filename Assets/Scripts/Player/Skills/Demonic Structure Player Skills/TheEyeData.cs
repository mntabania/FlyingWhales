using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheEyeData : DemonicStructurePlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.THE_EYE;

    public TheEyeData() {
        structureType = STRUCTURE_TYPE.THE_EYE;
    }
}
