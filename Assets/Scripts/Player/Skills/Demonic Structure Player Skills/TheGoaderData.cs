using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheGoaderData : DemonicStructurePlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.THE_GOADER;

    public TheGoaderData() {
        structureType = STRUCTURE_TYPE.THE_GOADER;
    }
}
