using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CryptData : DemonicStructurePlayerSkill {
    public override string name => "Crypt";
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.CRYPT;
    public override string description => "This Structure allows the Player to spawn Skeletons.";
    public CryptData() {
        structureType = STRUCTURE_TYPE.CRYPT;
    }
}