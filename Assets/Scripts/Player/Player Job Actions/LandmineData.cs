using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class LandmineData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.LANDMINE;
    public override string name { get { return "Landmine"; } }
    public override string description { get { return "Landmine"; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.DEVASTATION; } }
    public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 1;

    public LandmineData() : base() {
        targetTypes = new[] { SPELL_TARGET.TILE };
    }
    public override void ActivateAbility(LocationGridTile targetTile) {
        targetTile.SetHasLandmine(true);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile) {
        return !targetTile.hasLandmine;
    }
}