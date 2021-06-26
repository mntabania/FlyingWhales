using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using UtilityScripts;

public class ManifestFoodData : SkillData {

    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.MANIFEST_FOOD;
    public override string name => "Manifest Food";
    public override string description => "This Spell produces a pile of food out of thin air. Use it to lure characters.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 1;

    public ManifestFoodData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        TILE_OBJECT_TYPE tileObjectType = TILE_OBJECT_TYPE.FISH_PILE;
        if (currentLevel == 1) {
            tileObjectType = GameUtilities.RollChance(50) ? TILE_OBJECT_TYPE.FISH_PILE : TILE_OBJECT_TYPE.ANIMAL_MEAT;
        } else if (currentLevel == 2) {
            List<TILE_OBJECT_TYPE> choices = RuinarchListPool<TILE_OBJECT_TYPE>.Claim();
            choices.Add(TILE_OBJECT_TYPE.FISH_PILE);
            choices.Add(TILE_OBJECT_TYPE.ANIMAL_MEAT);
            choices.Add(TILE_OBJECT_TYPE.CORN);
            choices.Add(TILE_OBJECT_TYPE.POTATO);
            tileObjectType = CollectionUtilities.GetRandomElement(choices);
            RuinarchListPool<TILE_OBJECT_TYPE>.Release(choices);
        } else if (currentLevel == 3) {
            List<TILE_OBJECT_TYPE> choices = RuinarchListPool<TILE_OBJECT_TYPE>.Claim();
            choices.Add(TILE_OBJECT_TYPE.FISH_PILE);
            choices.Add(TILE_OBJECT_TYPE.ANIMAL_MEAT);
            choices.Add(TILE_OBJECT_TYPE.CORN);
            choices.Add(TILE_OBJECT_TYPE.POTATO);
            choices.Add(TILE_OBJECT_TYPE.ICEBERRY);
            choices.Add(TILE_OBJECT_TYPE.PINEAPPLE);
            tileObjectType = CollectionUtilities.GetRandomElement(choices);
            RuinarchListPool<TILE_OBJECT_TYPE>.Release(choices);
        }
        FoodPile foodPile = InnerMapManager.Instance.CreateNewTileObject<FoodPile>(tileObjectType);
        foodPile.SetResourceInPile((int)PlayerSkillManager.Instance.GetIncreaseStatsPercentagePerLevel(PLAYER_SKILL_TYPE.MANIFEST_FOOD));
        targetTile.structure.AddPOI(foodPile, targetTile);
        // foodPile.gridTileLocation.SetReservedType(TILE_OBJECT_TYPE.FOOD_PILE);
        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Smoke_Effect);
        //IncreaseThreatThatSeesTile(targetTile, 10);
        base.ActivateAbility(targetTile);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason) {
        bool canPerform = base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason);
        if (canPerform) {
            return targetTile.tileObjectComponent.objHere == null;
        }
        return canPerform;
    }
    public override void ShowValidHighlight(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(0, tile);
    }
}