using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.KILL;
    public override string name { get { return "Kill"; } }
    public override string description { get { return "Kill"; } }

    public KillData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }
}
