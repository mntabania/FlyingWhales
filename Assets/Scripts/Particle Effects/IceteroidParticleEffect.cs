using System.Collections;
using UnityEngine;
using Traits;

public class IceteroidParticleEffect : BaseParticleEffect {
    protected override void PlayParticle() {
        base.PlayParticle();
        StartCoroutine(IceteroidEffect());
    }
    private IEnumerator IceteroidEffect() {
        yield return new WaitForSeconds(0.6f);
        OnIceteroidFell();
    }
    protected override void ParticleAfterEffect(ParticleSystem particleSystem) {
        ObjectPoolManager.Instance.DestroyObject(gameObject);
    }
    public void OnIceteroidFell() {
        targetTile.PerformActionOnTraitables(DealDamage);
    }
    private void DealDamage(ITraitable traitable) {
        traitable.AdjustHP(-50, ELEMENTAL_TYPE.Ice, true, showHPBar: true);
        if (traitable is Character character && character.isDead == false && Random.Range(0, 100) < 25) {
            character.traitContainer.AddTrait(character, "Injured");
        }
    }
}
