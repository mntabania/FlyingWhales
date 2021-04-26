using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpHutData : DemonicStructurePlayerSkill {
    public override string name => "Imp Hut";
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.IMP_HUT;
    public override string description => "This Structure allows the Player to spawn Imps.";
    public ImpHutData() {
        structureType = STRUCTURE_TYPE.IMP_HUT;
    }
}