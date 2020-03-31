using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheCryptData : DemonicStructurePlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.THE_CRYPT;

    public TheCryptData() {
        structureType = STRUCTURE_TYPE.THE_CRYPT;
    }
}