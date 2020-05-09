using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonLustData : MinionPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.DEMON_LUST;
    public override string name => "Demon Lust";
    public override string description => "This Demon is a glass cannon magic-user that deals Fire damage. Can be summoned to Defend, Harass or Invade a target area or Assault a target character.";
    public DemonLustData() {
        className = "Lust";
    }
}
