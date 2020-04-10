﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbductData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.ABDUCT;
    public override string name { get { return "Abduct"; } }
    public override string description { get { return "Abduct"; } }

    public AbductData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }
}
