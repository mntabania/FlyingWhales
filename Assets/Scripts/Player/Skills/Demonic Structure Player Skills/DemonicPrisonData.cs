using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonicPrisonData : DemonicStructurePlayerSkill {
    public override string name => "Demonic Prison";
    public override SPELL_TYPE type => SPELL_TYPE.DEMONIC_PRISON;

    public DemonicPrisonData() {
        structureType = STRUCTURE_TYPE.DEMONIC_PRISON;
    }
}