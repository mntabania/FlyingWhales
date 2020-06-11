using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OstracizerData : DemonicStructurePlayerSkill {
    public override string name => "Ostracizer";
    public override SPELL_TYPE type => SPELL_TYPE.OSTRACIZER;
    public override string description =>
        $"This Structure allows the player to make most {UtilityScripts.Utilities.VillagerIcon()}Villagers " +
        $"look down on a smaller and specific subset of people. This is a powerful tool in fracturing a large society.";
    public OstracizerData() {
        structureType = STRUCTURE_TYPE.OSTRACIZER;
    }
}