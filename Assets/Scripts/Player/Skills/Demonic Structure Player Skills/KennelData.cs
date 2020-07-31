using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KennelData : DemonicStructurePlayerSkill {
    
    public override string name => "Kennel";
    public override SPELL_TYPE type => SPELL_TYPE.KENNEL;
    public override string description => "This Structure allows the player to breed wild monsters. These corrupted versions will have a different behavior from their wild counterpart. You can check a wild monster's expected Kennel-bred behavior from its Info Tab.";
    public KennelData() {
        structureType = STRUCTURE_TYPE.KENNEL;
    }
}