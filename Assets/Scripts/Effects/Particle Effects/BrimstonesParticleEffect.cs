﻿using System.Collections;
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
    protected virtual void ParticleAfterEffect(ParticleSystem particleSystem) {
        ObjectPoolManager.Instance.DestroyObject(gameObject);
    }
    public void OnBrimstoneFell() {
        // List<ITraitable> traitables = targetTile.GetTraitablesOnTile();
        targetTile.genericTileObject.traitContainer.AddTrait(targetTile.genericTileObject, "Danger Remnant");
        BurningSource bs = null;
        targetTile.PerformActionOnTraitables((traitable) => BrimstoneEffect(traitable, ref bs));
    }
    private void BrimstoneEffect(ITraitable traitable, ref BurningSource bs) {
        if (traitable is TileObject obj) {
            if (obj.tileObjectType == TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT) {
                BurningSource burningSource = bs;
                CombatManager.Instance.ApplyElementalDamage(0, ELEMENTAL_TYPE.Fire, obj,
                    elementalTraitProcessor: (target, trait) => TraitManager.Instance.ProcessBurningTrait(target, trait, ref burningSource));
                bs = burningSource;
            } else {
                BurningSource burningSource = bs;
                obj.AdjustHP(-400, ELEMENTAL_TYPE.Fire, true, elementalTraitProcessor: (target, trait) => TraitManager.Instance.ProcessBurningTrait(target, trait, ref burningSource), showHPBar: true);
                bs = burningSource;
            }
        } else if (traitable is Character character) {
            BurningSource burningSource = bs;
            character.AdjustHP(-400, ELEMENTAL_TYPE.Fire, true, 
                elementalTraitProcessor: (target, trait) => TraitManager.Instance.ProcessBurningTrait(target, trait, ref burningSource), showHPBar: true);
            bs = burningSource;
            if (Random.Range(0, 100) < 25) {
                character.traitContainer.AddTrait(character, "Injured");
            }
        } else {
            BurningSource burningSource = bs;
            traitable.AdjustHP(-400, ELEMENTAL_TYPE.Fire, true, 
                elementalTraitProcessor: (target, trait) => TraitManager.Instance.ProcessBurningTrait(target, trait, ref burningSource), showHPBar: true);
            bs = burningSource;
        }
    }
}
