using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmpowerData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.EMPOWER;
    public override string name => "Empower";
    public override string description => "This Action will significantly increase a character's combat prowess for a short amount of time.";
    public EmpowerData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }
}
