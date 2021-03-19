﻿using System.Collections;
using Scriptable_Object_Scripts;
using UnityEngine;
using Traits;
using UtilityScripts;

public class IceteroidParticleEffect : BaseParticleEffect {
    protected override void PlayParticle() {
        base.PlayParticle();
        StartCoroutine(IceteroidEffect());
    }
    private IEnumerator IceteroidEffect() {
        yield return new WaitForSeconds(1.6f);
        OnIceteroidFell();
    }
    protected virtual void ParticleAfterEffect(ParticleSystem particleSystem) {
        ObjectPoolManager.Instance.DestroyObject(gameObject);
    }
    public void OnIceteroidFell() {
        AudioManager.Instance.TryCreateAudioObject(
            CollectionUtilities.GetRandomElement(
                PlayerSkillManager.Instance.GetPlayerSkillData<IceteroidSkillData>(PLAYER_SKILL_TYPE.ICETEROIDS).impactClips
            ), 
            targetTile, 1, false
        );
        targetTile.tileObjectComponent.genericTileObject.traitContainer.AddTrait(targetTile.tileObjectComponent.genericTileObject, "Danger Remnant");
        targetTile.PerformActionOnTraitables(DealDamage);
    }
    private void DealDamage(ITraitable traitable) {
        int additionalDamage = PlayerSkillManager.Instance.GetAdditionalDamageBaseOnLevel(PLAYER_SKILL_TYPE.ICETEROIDS);
        int processedDamage = additionalDamage;
        traitable.AdjustHP(-processedDamage, ELEMENTAL_TYPE.Ice, true, showHPBar: true,
                    piercingPower: PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.ICETEROIDS));
        //traitable.AdjustHP(-400, ELEMENTAL_TYPE.Ice, true, showHPBar: true);
        if (traitable is Character character && character.isDead == false) {
            traitable.traitContainer.AddTrait(traitable, "Freezing", null, null, true);
            traitable.traitContainer.AddTrait(traitable, "Freezing", null, null, true);
            traitable.traitContainer.AddTrait(traitable, "Freezing", null, null, true);
            if (Random.Range(0, 100) < 25) {
                character.traitContainer.AddTrait(character, "Injured");
            }
            Messenger.Broadcast(PlayerSignals.PLAYER_HIT_CHARACTER_VIA_SPELL, character, processedDamage);
            if (character.isDead && character.skillCauseOfDeath == PLAYER_SKILL_TYPE.NONE) {
                character.skillCauseOfDeath = PLAYER_SKILL_TYPE.ICETEROIDS;
                Messenger.Broadcast(PlayerSignals.CREATE_SPIRIT_ENERGY, character.deathTilePosition.centeredWorldLocation, 1, character.deathTilePosition.parentMap);
            }
        }
    }
}
