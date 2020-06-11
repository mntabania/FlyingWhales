using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeData : DemonicStructurePlayerSkill {
    public override string name => "Eye";
    public override SPELL_TYPE type => SPELL_TYPE.EYE;
    public override string description => $"This Structure notifies the player of important events that occur within its region. " +
                                          $"Some of these events can then be stored as an Intel that can be shared to {UtilityScripts.Utilities.VillagerIcon()}Villagers.";
    public EyeData() {
        structureType = STRUCTURE_TYPE.EYE;
    }
}
