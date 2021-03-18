using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Traits;
using Inner_Maps;

public class BrimstonesParticleEffect : BaseParticleEffect {

    private int m_brimstoneBaseDamage = -400;
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
        targetTile.tileObjectComponent.genericTileObject.traitContainer.AddTrait(targetTile.tileObjectComponent.genericTileObject, "Danger Remnant");
        BurningSource bs = null;
        targetTile.PerformActionOnTraitables((traitable) => BrimstoneEffect(traitable, ref bs));
    }
    private void BrimstoneEffect(ITraitable traitable, ref BurningSource bs) {
        int additionalDamage = PlayerSkillManager.Instance.GetAdditionalDamageBaseOnLevel(PLAYER_SKILL_TYPE.BRIMSTONES);
        if(additionalDamage > 0) {
            additionalDamage *= -1;
        }
        int processedDamage = m_brimstoneBaseDamage + additionalDamage;
        if (traitable is TileObject obj) {
            //int processedDamage = m_brimstoneBaseDamage - (m_brimstoneBaseDamage * PlayerSkillManager.Instance.GetAdditionalDamageBaseOnLevel(PLAYER_SKILL_TYPE.BRIMSTONES));
            if (obj.tileObjectType == TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT) {
                BurningSource burningSource = bs;
                CombatManager.Instance.ApplyElementalDamage(0, ELEMENTAL_TYPE.Fire, obj,
                    elementalTraitProcessor: (target, trait) => TraitManager.Instance.ProcessBurningTrait(target, trait, ref burningSource));
                bs = burningSource;
            } else {
                BurningSource burningSource = bs;

                obj.AdjustHP(processedDamage, ELEMENTAL_TYPE.Fire, true, elementalTraitProcessor: (target, trait) => TraitManager.Instance.ProcessBurningTrait(target, trait, ref burningSource), showHPBar: true);
                bs = burningSource;
            }
        } else if (traitable is Character character) {
            //int processedDamage = m_brimstoneBaseDamage - (m_brimstoneBaseDamage * PlayerSkillManager.Instance.GetAdditionalDamageBaseOnLevel(PLAYER_SKILL_TYPE.BRIMSTONES));
            BurningSource burningSource = bs;
            character.AdjustHP(processedDamage, ELEMENTAL_TYPE.Fire, true, 
                elementalTraitProcessor: (target, trait) => TraitManager.Instance.ProcessBurningTrait(target, trait, ref burningSource), showHPBar: true);
            bs = burningSource;
            Messenger.Broadcast(PlayerSignals.PLAYER_HIT_CHARACTER_VIA_SPELL, character, processedDamage);
            if (character.isDead) {
                character.skillCauseOfDeath = PLAYER_SKILL_TYPE.BRIMSTONES;
                Messenger.Broadcast(PlayerSignals.CREATE_SPIRIT_ENERGY, character.deathTilePosition.centeredWorldLocation, 1, character.deathTilePosition.parentMap);
            }
            if (Random.Range(0, 100) < 25) {
                character.traitContainer.AddTrait(character, "Injured");
            }
        } else {
            BurningSource burningSource = bs;
            //int processedDamage = m_brimstoneBaseDamage - (m_brimstoneBaseDamage * PlayerSkillManager.Instance.GetAdditionalDamageBaseOnLevel(PLAYER_SKILL_TYPE.BRIMSTONES));
            traitable.AdjustHP(processedDamage, ELEMENTAL_TYPE.Fire, true, 
                elementalTraitProcessor: (target, trait) => TraitManager.Instance.ProcessBurningTrait(target, trait, ref burningSource), showHPBar: true);
            bs = burningSource;
        }
    }
}
