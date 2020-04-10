using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.ANIMATE;
    public override string name { get { return "Animate"; } }
    public override string description { get { return "Animate"; } }

    public AnimateData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }
}
