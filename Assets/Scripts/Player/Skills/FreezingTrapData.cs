using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class FreezingTrapData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.FREEZING_TRAP;
    public override string name { get { return "Freezing Trap"; } }
    public override string description { get { return "Freezing Trap"; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.SPELL; } }
    public virtual int abilityRadius => 1;

    public FreezingTrapData() : base() {
        targetTypes = new[] { SPELL_TARGET.TILE };
    }
    public override void ActivateAbility(LocationGridTile targetTile) {
        targetTile.SetHasFreezingTrap(true);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile) {
        return !targetTile.hasFreezingTrap;
    }
}