using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TortureChamberData : DemonicStructurePlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.TORTURE_CHAMBER;

    public TortureChamberData() {
        structureType = STRUCTURE_TYPE.TORTURE_CHAMBER;
    }
}