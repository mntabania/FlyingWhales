using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonGluttonyData : MinionPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.DEMON_GLUTTONY;
    public override string name => "Demon Gluttony";
    public override string description => "This Demon is a robust ranged magic-user that deals Water damage. Can be summoned to defend an Area or Structure.";
    public DemonGluttonyData() {
        className = "Gluttony";
    }
}
