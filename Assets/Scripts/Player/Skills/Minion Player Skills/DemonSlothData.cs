using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonSlothData : MinionPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.DEMON_SLOTH;
    public override string name { get { return "Demon Sloth"; } }
    public override string description { get { return "This Demon is a tough melee magic-user that deals Ice damage. Can be summoned to Defend, Harass or Invade a target area or Assault a target character."; } }

    public DemonSlothData() {
        className = "Sloth";
    }
}
