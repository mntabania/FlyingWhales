using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonEnvyData : MinionPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.DEMON_ENVY;
    public override string name => "Demon Envy";
    public override string description => "This Demon is a wily ranged archer that deals Poison damage. Can be summoned to Defend, Harass or Invade a target area or Assault a target character.";
    public DemonEnvyData() {
        className = "Envy";
    }
}
