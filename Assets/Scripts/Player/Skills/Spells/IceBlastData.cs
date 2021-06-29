using System.Collections.Generic;
using Inner_Maps;
using Scriptable_Object_Scripts;
using Traits;
using UtilityScripts;

public class IceBlastData : SkillData {

    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.ICE_BLAST;
    public override string name => "Ice Blast";
    public override string description => "This Spell causes cold spikes to shatter around a small area, dealing Ice damage to those within range.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

    public IceBlastData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        AudioManager.Instance.TryCreateAudioObject(
            PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<IceBlastSkillData>(PLAYER_SKILL_TYPE.ICE_BLAST).blastSound,
            targetTile, 3, false
        );
        int processedDamage = -PlayerSkillManager.Instance.GetDamageBaseOnLevel(this);
        int processedTileRange = PlayerSkillManager.Instance.GetTileRangeBonusPerLevel(this);
        float piercing = PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(this);
        UnityEngine.GameObject go = GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Ice_Blast);
        go.transform.localScale = new UnityEngine.Vector3(go.transform.localScale.x * processedTileRange, go.transform.localScale.y * processedTileRange, go.transform.localScale.z * processedTileRange);
        List<LocationGridTile> tiles = RuinarchListPool<LocationGridTile>.Claim();
        targetTile.PopulateTilesInRadius(tiles, processedTileRange, includeCenterTile: true, includeTilesInDifferentStructure: true);
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            tile.PerformActionOnTraitables((traitable) => ApplyIceDamage(traitable, processedDamage, piercing));
        }
        targetTile.tileObjectComponent.genericTileObject.traitContainer.AddTrait(targetTile.tileObjectComponent.genericTileObject, "Danger Remnant");
        //IncreaseThreatThatSeesTile(targetTile, 10);
        RuinarchListPool<LocationGridTile>.Release(tiles);
        base.ActivateAbility(targetTile);
    }
    private void ApplyIceDamage(ITraitable traitable, int processedDamage, float piercing) {
        traitable.AdjustHP(processedDamage, ELEMENTAL_TYPE.Ice, true, showHPBar: true, piercingPower: piercing, isPlayerSource: true, source : this);

        if (traitable is Character character) {
            Messenger.Broadcast(PlayerSignals.PLAYER_HIT_CHARACTER_VIA_SPELL, character, processedDamage);
            if (character.isDead && character.skillCauseOfDeath == PLAYER_SKILL_TYPE.NONE) {
                character.skillCauseOfDeath = PLAYER_SKILL_TYPE.ICE_BLAST;
                //Messenger.Broadcast(PlayerSignals.CREATE_SPIRIT_ENERGY, character.deathTilePosition.centeredWorldLocation, 1, character.deathTilePosition.parentMap);
                //Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, character.deathTilePosition.centeredWorldLocation, 1, character.deathTilePosition.parentMap);
            }
        }
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason) {
        bool canPerform = base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason);
        if (canPerform) {
            return targetTile.structure != null;
        }
        return canPerform;
    }
    public override void ShowValidHighlight(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(PlayerSkillManager.Instance.GetTileRangeBonusPerLevel(PLAYER_SKILL_TYPE.ICE_BLAST), tile);
    }
}