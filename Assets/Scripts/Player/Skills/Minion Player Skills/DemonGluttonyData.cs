using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonGluttonyData : MinionPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.DEMON_GLUTTONY;
    public override string name { get { return "Demon Gluttony"; } }
    public override string description { get { return "This Demon is a robust ranged magic-user that deals Water damage. Can be summoned to Defend, Harass or Invade a target area or Assault a target character."; } }

    public DemonGluttonyData() {
        className = "Gluttony";
    }
}
