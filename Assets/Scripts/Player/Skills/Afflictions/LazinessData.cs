using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class LazinessData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.LAZINESS;
    public override string name => "Laziness";
    public override string description => "This Affliction will make a Villager Lazy. Lazy villagers may sometimes refuse to do work.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.AFFLICTION;
    public LazinessData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }
}