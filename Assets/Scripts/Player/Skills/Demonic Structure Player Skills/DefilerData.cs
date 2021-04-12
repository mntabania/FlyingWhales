using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefilerData : DemonicStructurePlayerSkill {
    public override string name => "Defiler";
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DEFILER;
    public override string description => "Gives the player access to Vampirism Affliction and Spawn Necronomicon Spell. Allows the player to gain Chaos Orbs from Vampire and Necromancer activities.";
    public DefilerData() {
        structureType = STRUCTURE_TYPE.DEFILER;
    }
}
