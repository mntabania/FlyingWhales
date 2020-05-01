using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonGreedData : MinionPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.DEMON_GREED;
    public override string name { get { return "Demon Greed"; } }
    public override string description { get { return "This Demon is a harsh melee combatant that deals Wind damage. Can be summoned to Defend, Harass or Invade a target area or Assault a target character."; } }

    public DemonGreedData() {
        className = "Greed";
    }
}
