using System.Collections.Generic;
using Inner_Maps;
using Scriptable_Object_Scripts;
using Traits;
using UtilityScripts;

public class SplashPoisonData : SkillData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SPLASH_POISON;
    public override string name => "Splash Poison";
    public override string description => "This Spell applies Poison on a small area.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;

    public SplashPoisonData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        AudioManager.Instance.TryCreateAudioObject(
            CollectionUtilities.GetRandomElement(PlayerSkillManager.Instance
                .GetScriptableObjPlayerSkillData<SplashPoisonSkillData>(PLAYER_SKILL_TYPE.SPLASH_POISON).splashSounds),
            targetTile, 3, false
        );
        List<LocationGridTile> tiles = RuinarchListPool<LocationGridTile>.Claim();
        targetTile.PopulateTilesInRadius(tiles, 1, includeCenterTile: true, includeTilesInDifferentStructure: true);
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            tile.PerformActionOnTraitables(MakeTraitblePoisoned);
        }
        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Poison_Bomb);
        targetTile.tileObjectComponent.genericTileObject.traitContainer.AddTrait(targetTile.tileObjectComponent.genericTileObject, "Surprised Remnant");
        //IncreaseThreatThatSeesTile(targetTile, 10);
        RuinarchListPool<LocationGridTile>.Release(tiles);
        base.ActivateAbility(targetTile);
    }
    private void MakeTraitblePoisoned(ITraitable traitable) {
        traitable.traitContainer.AddTrait(traitable, "Poisoned", bypassElementalChance: true, overrideDuration: GetDurationBaseOnTileType(traitable));
        Poisoned poisoned = traitable.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned");
        poisoned?.SetIsPlayerSource(true);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason) {
        bool canPerform = base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason);
        if (canPerform) {
            return targetTile.structure != null;
        }
        return canPerform;
    }
    public override void ShowValidHighlight(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(1, tile);
    }

    public int GetDurationBaseOnTileType(ITraitable p_targetTile) {
        int baseTick = PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.SPLASH_POISON);
        if (p_targetTile is GenericTileObject genericTileObject) {
            genericTileObject.AddAdvertisedAction(INTERACTION_TYPE.CLEANSE_TILE);
            if (genericTileObject.gridTileLocation.groundType == LocationGridTile.Ground_Type.Desert_Grass ||
                genericTileObject.gridTileLocation.groundType == LocationGridTile.Ground_Type.Desert_Stone ||
                genericTileObject.gridTileLocation.groundType == LocationGridTile.Ground_Type.Sand) {
                //Reduce duration of poison when put on desert tiles

                baseTick = GameManager.Instance.GetTicksBasedOnHour(2);
            }
        }
        return baseTick;
    }
}

