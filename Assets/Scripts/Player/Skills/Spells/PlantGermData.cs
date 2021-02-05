﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class PlantGermData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.PLANT_GERM;
    public override string name => "Plant Germ";
    public override string description => "This Action puts an Abomination Germ on a Mushroom or a Berry Shrub.";

    public PlantGermData() : base() {
        targetTypes = new[] { SPELL_TARGET.TILE_OBJECT };
    }
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        base.ActivateAbility(targetPOI);
        targetPOI.traitContainer.AddTrait(targetPOI, "Abomination Germ");
    }
    public override bool CanPerformAbilityTowards(TileObject tileObject) {
        if (base.CanPerformAbilityTowards(tileObject)) {
            return tileObject.traitContainer.HasTrait("Abomination Germ") == false;
        }
        return false;
    }
}