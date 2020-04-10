using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class HotheadedData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.HOTHEADED;
    public override string name { get { return "Hotheaded"; } }
    public override string description { get { return "Hotheaded"; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.AFFLICTION; } }

    public HotheadedData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }
}