using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefilerData : DemonicStructurePlayerSkill {
    public override string name => "Defiler";
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DEFILER;
    public override string description => "This Structure allows the player to brainwash Villagers into Cultists. These demonic Cultists can then be manipulated to perform certain helpful actions for you.";
    public DefilerData() {
        structureType = STRUCTURE_TYPE.DEFILER;
    }
}
