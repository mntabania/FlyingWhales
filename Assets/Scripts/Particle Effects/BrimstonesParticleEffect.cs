using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Traits;
using Inner_Maps;

public class BrimstonesParticleEffect : BaseParticleEffect {
    protected override void PlayParticle() {
        base.PlayParticle();
        StartCoroutine(BrimstoneEffect());
    }
    private IEnumerator BrimstoneEffect() {
        yield return new WaitForSeconds(0.6f);
        OnBrimstoneFell();
    }
    protected override void ParticleAfterEffect(ParticleSystem particleSystem) {
        ObjectPoolManager.Instance.DestroyObject(gameObject);
    }
    public void OnBrimstoneFell() {
        List<ITraitable> traitables = targetTile.GetTraitablesOnTile();
        BurningSource bs = null;
        for (int i = 0; i < traitables.Count; i++) {
            ITraitable traitable = traitables[i];
            if (traitable is TileObject obj) {
                if (obj.tileObjectType != TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT) {
                    obj.AdjustHP(-obj.currentHP, ELEMENTAL_TYPE.Fire, elementalTraitProcessor: (target, trait) => ProcessBurningTrait(target, trait, ref bs));
                    if (obj.gridTileLocation == null) {
                        continue; //object was destroyed, do not add burning trait
                    }
                } else {
                    CombatManager.Instance.ApplyElementalDamage(0, ELEMENTAL_TYPE.Fire, obj,
                        elementalTraitProcessor: (target, trait) => ProcessBurningTrait(target, trait, ref bs));
                }
            } else if (traitable is Character character) {
                character.AdjustHP(-(int) (character.maxHP * 0.4f), ELEMENTAL_TYPE.Fire, true, 
                    elementalTraitProcessor: (target, trait) => ProcessBurningTrait(target, trait, ref bs));
                if (Random.Range(0, 100) < 25) {
                    character.traitContainer.AddTrait(character, "Injured");
                }
            } else {
                traitable.AdjustHP(-traitable.currentHP, ELEMENTAL_TYPE.Fire, 
                    elementalTraitProcessor: (target, trait) => ProcessBurningTrait(target, trait, ref bs));
            }
            // Burning burningTrait = traitable.traitContainer.GetNormalTrait<Burning>("Burning");
            // if (burningTrait != null && burningTrait.sourceOfBurning == null) {
            //     if (bs == null) {
            //         bs = new BurningSource(traitable.gridTileLocation.parentMap.location);
            //     }
            //     burningTrait.SetSourceOfBurning(bs, traitable);
            // }
        }
    }

    private void ProcessBurningTrait(ITraitable traitable, Trait trait, ref BurningSource burningSource) {
        if (trait is Burning burning) {
            if (burningSource == null) {
                burningSource = new BurningSource(traitable.gridTileLocation.parentMap.region);
            }
            burning.SetSourceOfBurning(burningSource, traitable);
        }
    }
}
