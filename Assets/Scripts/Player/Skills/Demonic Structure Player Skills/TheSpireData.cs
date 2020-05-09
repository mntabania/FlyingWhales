using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheSpireData : DemonicStructurePlayerSkill {
    public override string name => "The Spire";
    public override SPELL_TYPE type => SPELL_TYPE.THE_SPIRE;

    public TheSpireData() {
        structureType = STRUCTURE_TYPE.THE_SPIRE;
    }
}