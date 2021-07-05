using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Scriptable_Object_Scripts;
using Traits;
using UtilityScripts;

public class LightningData : SkillData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.LIGHTNING;
    public override string name => "Lightning";
    public override string description => "This Spell triggers a lightning strike at the target spot. Deals major Electric damage.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 1;

    public LightningData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        AudioManager.Instance.TryCreateAudioObject(
            CollectionUtilities.GetRandomElement(PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<LightningSkillData>(PLAYER_SKILL_TYPE.LIGHTNING).thunderAudioClips), 
            targetTile, 1, false
        );
        int processedDamage = -PlayerSkillManager.Instance.GetDamageBaseOnLevel(this);
        float piercing = PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(this);
        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Lightning_Strike);
        targetTile.PerformActionOnTraitables((t) => LightningDamage(t, processedDamage, piercing));
        targetTile.tileObjectComponent.genericTileObject.traitContainer.AddTrait(targetTile.tileObjectComponent.genericTileObject, "Lightning Remnant");

        List<LocationGridTile> crossNeighbours = targetTile.FourNeighbours();
        for (int i = 0; i < crossNeighbours.Count; i++) {
            LocationGridTile neighbour = crossNeighbours[i];
            neighbour.PerformActionOnTraitables((t) => LightningDamage(t, processedDamage, piercing));
        }
        // List<IPointOfInterest> pois = targetTile.GetPOIsOnTile();
        // for (int i = 0; i < pois.Count; i++) {
        //     pois[i].AdjustHP(-350, ELEMENTAL_TYPE.Electric, showHPBar: true);
        // }
        //IncreaseThreatThatSeesTile(targetTile, 10);
        base.ActivateAbility(targetTile);
    }
    private void LightningDamage(ITraitable traitable, int processedDamage, float piercing) {
        if (traitable is IPointOfInterest poi) {
            poi.AdjustHP(processedDamage, ELEMENTAL_TYPE.Electric, triggerDeath: true, showHPBar: true, piercingPower: piercing, isPlayerSource: true, source: this);
            Messenger.Broadcast(PlayerSignals.PLAYER_HIT_CHARACTER_VIA_SPELL, traitable as Character, processedDamage);
            if (traitable is Character character && character.isDead && character.skillCauseOfDeath == PLAYER_SKILL_TYPE.NONE) {
                character.skillCauseOfDeath = PLAYER_SKILL_TYPE.LIGHTNING;
                //Messenger.Broadcast(PlayerSignals.CREATE_SPIRIT_ENERGY, character.deathTilePosition.centeredWorldLocation, 1, character.deathTilePosition.parentMap);
                //Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, character.deathTilePosition.centeredWorldLocation, 1, character.deathTilePosition.parentMap);
            }
        }
    }
    public override void ShowValidHighlight(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(0, tile);
    }
}
