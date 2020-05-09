using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgitateData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.AGITATE;
    public override string name => "Agitate";
    public override string description => "This Action can be used on a monster. The target will enter a state of frenzy and will terrorize nearby Residents.";
    public AgitateData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }
}
