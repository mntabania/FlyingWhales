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

    private int m_lightningBaseDamage = -600;

    public LightningData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        AudioManager.Instance.TryCreateAudioObject(
            CollectionUtilities.GetRandomElement(PlayerSkillManager.Instance.GetPlayerSkillData<LightningSkillData>(PLAYER_SKILL_TYPE.LIGHTNING).thunderAudioClips), 
            targetTile, 1, false
        );
        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Lightning_Strike);
        targetTile.PerformActionOnTraitables(LightningDamage);
        targetTile.genericTileObject.traitContainer.AddTrait(targetTile.genericTileObject, "Lightning Remnant");

        List<LocationGridTile> crossNeighbours = targetTile.GetCrossNeighbours();
        for (int i = 0; i < crossNeighbours.Count; i++) {
            LocationGridTile neighbour = crossNeighbours[i];
            neighbour.PerformActionOnTraitables(LightningDamage);
        }
        // List<IPointOfInterest> pois = targetTile.GetPOIsOnTile();
        // for (int i = 0; i < pois.Count; i++) {
        //     pois[i].AdjustHP(-350, ELEMENTAL_TYPE.Electric, showHPBar: true);
        // }
        //IncreaseThreatThatSeesTile(targetTile, 10);
        base.ActivateAbility(targetTile);
    }
    private void LightningDamage(ITraitable traitable) {
        if (traitable is IPointOfInterest poi) {
            int processedDamage = m_lightningBaseDamage - PlayerSkillManager.Instance.GetAdditionalDamageBaseOnLevel(PLAYER_SKILL_TYPE.LIGHTNING);
            poi.AdjustHP(processedDamage, ELEMENTAL_TYPE.Electric, triggerDeath: true, showHPBar: true);
            if (traitable is Character character && traitable.currentHP <= 0) {
                (character).skillCauseOfDeath = PLAYER_SKILL_TYPE.LIGHTNING;
                Messenger.Broadcast(PlayerSignals.CREATE_SPIRIT_ENERGY, character.marker.transform.position, 1, character.currentRegion.innerMap);
            }
        }
    }
    public override void ShowValidHighlight(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(0, tile);
    }
}
