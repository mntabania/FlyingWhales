using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefilerData : DemonicStructurePlayerSkill {
    public override string name => "Defiler";
    public override SPELL_TYPE type => SPELL_TYPE.DEFILER;
    public override string description =>
        $"This Structure allows the player to turn Villagers with a dark side into Cultists. " +
        "These demonic Cultists can then be manipulated to perform certain helpful actions for you.";
    public DefilerData() {
        structureType = STRUCTURE_TYPE.DEFILER;
    }
}
