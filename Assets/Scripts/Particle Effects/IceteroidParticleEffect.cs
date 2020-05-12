using System.Collections;
using UnityEngine;
using Traits;

public class IceteroidParticleEffect : BaseParticleEffect {
    protected override void PlayParticle() {
        base.PlayParticle();
        StartCoroutine(IceteroidEffect());
    }
    private IEnumerator IceteroidEffect() {
        yield return new WaitForSeconds(1.6f);
        OnIceteroidFell();
    }
    protected override void ParticleAfterEffect(ParticleSystem particleSystem) {
        ObjectPoolManager.Instance.DestroyObject(gameObject);
    }
    public void OnIceteroidFell() {
        targetTile.PerformActionOnTraitables(DealDamage);
    }
    private void DealDamage(ITraitable traitable) {
        traitable.AdjustHP(-240, ELEMENTAL_TYPE.Ice, true, showHPBar: true);
        if (traitable is Character character && character.isDead == false) {
            traitable.traitContainer.AddTrait(traitable, "Freezing", null, null, true);
            traitable.traitContainer.AddTrait(traitable, "Freezing", null, null, true);
            if (Random.Range(0, 100) < 25) {
                character.traitContainer.AddTrait(character, "Injured");
            }
        }
    }
}
