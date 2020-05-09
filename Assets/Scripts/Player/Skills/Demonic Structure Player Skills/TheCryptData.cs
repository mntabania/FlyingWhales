using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheCryptData : DemonicStructurePlayerSkill {
    public override string name => "The Crypt";
    public override SPELL_TYPE type => SPELL_TYPE.THE_CRYPT;
    public override string description => "This Structure allows the player to store an item for future use. Any item stored within the Crypt will be retained once the world has been invaded. You can store up to 3 items.";
    public TheCryptData() {
        structureType = STRUCTURE_TYPE.THE_CRYPT;
    }
}