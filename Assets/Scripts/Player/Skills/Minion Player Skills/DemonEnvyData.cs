using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonEnvyData : MinionPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.DEMON_ENVY;
    public override string name => "Demon Envy";
    public override string description => "This Demon is a wily ranged archer that deals Poison damage. Roams around the area it was spawned and will disable hostiles that enter. They may also sometimes follow and assist Invader-type minions.";
    public DemonEnvyData() {
        className = "Envy";
    }
}
