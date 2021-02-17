using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
public class ManifestFoodData : SkillData {

    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.MANIFEST_FOOD;
    public override string name => "Manifest Food";
    public override string description => "This Spell produces a pile of food out of thin air. Use it to lure characters.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

    private int m_baseAmountOfResources = 15;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 1;

    public ManifestFoodData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        FoodPile foodPile = InnerMapManager.Instance.CreateNewTileObject<FoodPile>(TILE_OBJECT_TYPE.ANIMAL_MEAT);
        m_baseAmountOfResources += (int)PlayerSkillManager.Instance.GetIncreaseStatsPercentagePerLevel(PLAYER_SKILL_TYPE.MANIFEST_FOOD);
        foodPile.SetResourceInPile(m_baseAmountOfResources);
        targetTile.structure.AddPOI(foodPile, targetTile);
        // foodPile.gridTileLocation.SetReservedType(TILE_OBJECT_TYPE.FOOD_PILE);
        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Smoke_Effect);
        //IncreaseThreatThatSeesTile(targetTile, 10);
        base.ActivateAbility(targetTile);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason) {
        bool canPerform = base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason);
        if (canPerform) {
            return targetTile.objHere == null;
        }
        return canPerform;
    }
    public override void ShowValidHighlight(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(0, tile);
    }
}