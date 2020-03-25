using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class SnareTrapData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.SNARE_TRAP;
    public override string name { get { return "Snare Trap"; } }
    public override string description { get { return "Snare Trap"; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.SPELL; } }
    public virtual int abilityRadius => 1;

    public SnareTrapData() : base() {
        targetTypes = new[] { SPELL_TARGET.TILE };
    }
    public override void ActivateAbility(LocationGridTile targetTile) {
        targetTile.SetHasSnareTrap(true);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile) {
        return !targetTile.hasSnareTrap;
    }
}