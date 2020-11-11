using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiolabData : DemonicStructurePlayerSkill {
    public override string name => "Biolab";
    public override SPELL_TYPE type => SPELL_TYPE.BIOLAB;
    public override string description => "This Structure allows the player to customize a plague using Plague Points.";
    public BiolabData() {
        structureType = STRUCTURE_TYPE.BIOLAB;
    }
}
