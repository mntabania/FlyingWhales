using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
public class ManifestFoodData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.MANIFEST_FOOD;
    public override string name { get { return "Manifest Food"; } }
    public override string description { get { return "Produce a small amount of Food."; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.DEVASTATION; } }
    public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public override int abilityRadius => 1;

    public ManifestFoodData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        FoodPile foodPile = InnerMapManager.Instance.CreateNewTileObject<FoodPile>(TILE_OBJECT_TYPE.FOOD_PILE);
        foodPile.SetResourceInPile(15);
        targetTile.structure.AddPOI(foodPile, targetTile);
        foodPile.gridTileLocation.SetReservedType(TILE_OBJECT_TYPE.FOOD_PILE);
        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Smoke_Effect);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile) {
        return targetTile.objHere == null;
    }
}