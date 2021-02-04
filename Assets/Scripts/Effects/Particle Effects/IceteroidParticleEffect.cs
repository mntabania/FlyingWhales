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
        targetTile.genericTileObject.traitContainer.AddTrait(targetTile.genericTileObject, "Danger Remnant");
        targetTile.PerformActionOnTraitables(DealDamage);
    }
    private void DealDamage(ITraitable traitable) {
        traitable.AdjustHP(-400, ELEMENTAL_TYPE.Ice, true, showHPBar: true);
        if (traitable is Character character && character.isDead == false) {
            traitable.traitContainer.AddTrait(traitable, "Freezing", null, null, true);
            traitable.traitContainer.AddTrait(traitable, "Freezing", null, null, true);
            traitable.traitContainer.AddTrait(traitable, "Freezing", null, null, true);
            if (Random.Range(0, 100) < 25) {
                character.traitContainer.AddTrait(character, "Injured");
            }
        }
    }
}
