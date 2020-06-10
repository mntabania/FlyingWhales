using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CryptData : DemonicStructurePlayerSkill {
    public override string name => "Crypt";
    public override SPELL_TYPE type => SPELL_TYPE.CRYPT;
    public override string description => "This Structure allows the player to store powerful artifacts and activate their hidden power.";
    public CryptData() {
        structureType = STRUCTURE_TYPE.CRYPT;
    }
}