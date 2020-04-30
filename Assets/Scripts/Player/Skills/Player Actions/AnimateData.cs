using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.ANIMATE;
    public override string name { get { return "Animate"; } }
    public override string description { get { return "This Action can be used on an object. The target will be transformed into a hostile monster that will attack any Resident that gets near it."; } }

    public AnimateData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }
}
