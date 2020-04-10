using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgitateData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.AGITATE;
    public override string name { get { return "Agitate"; } }
    public override string description { get { return "Agitate"; } }

    public AgitateData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }
}
