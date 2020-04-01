using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheKennelData : DemonicStructurePlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.THE_KENNEL;

    public TheKennelData() {
        structureType = STRUCTURE_TYPE.THE_KENNEL;
    }
}