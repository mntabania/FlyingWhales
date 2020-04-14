using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmpowerData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.EMPOWER;
    public override string name { get { return "Empower"; } }
    public override string description { get { return "Empower"; } }

    public EmpowerData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }
}
