using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class DefensePointData : DemonicStructurePlayerSkill {
    public override string name => "Prism";
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DEFENSE_POINT;
    public override string description => "This Structure allows the Player to summon defense parties.";
    public DefensePointData() {
        structureType = STRUCTURE_TYPE.DEFENSE_POINT;
    }
}
