using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TortureChambersData : DemonicStructurePlayerSkill {
    public override string name => "Prison";
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.TORTURE_CHAMBERS;
    public override string description => "This Structure allows the Player to capture Villagers. Imprisoned Villagers may be tortured, brainwashed or be drained of Spirit Energy.";
    public TortureChambersData() {
        structureType = STRUCTURE_TYPE.TORTURE_CHAMBERS;
    }
}