using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Traits;
using Inner_Maps;

public class BrimstonesParticleEffect : BaseParticleEffect {

    public bool IsCastedByPlayer { set; get; }

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
        SkillData brimstonesData = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.BRIMSTONES);
        float piercing = 0f;
        int additionalDamage;
        if (!IsCastedByPlayer) {
            additionalDamage = -PlayerSkillManager.Instance.GetDamageBaseOnLevel(brimstonesData, 0);
        } else {
            additionalDamage = -PlayerSkillManager.Instance.GetDamageBaseOnLevel(brimstonesData);
            piercing = PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(brimstonesData);
        }
        BurningSource bs = null;
        targetTile.PerformActionOnTraitables((traitable) => BrimstoneEffect(traitable, additionalDamage, piercing, brimstonesData, ref bs));
    }
    private void BrimstoneEffect(ITraitable traitable, int additionalDamage, float piercing, SkillData brimstonesData, ref BurningSource bs) {
        int processedDamage = additionalDamage;
        if (traitable is TileObject obj) {
            //int processedDamage = m_brimstoneBaseDamage - (m_brimstoneBaseDamage * PlayerSkillManager.Instance.GetAdditionalDamageBaseOnLevel(PLAYER_SKILL_TYPE.BRIMSTONES));
            if (obj.tileObjectType == TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT) {
                BurningSource burningSource = bs;
                CombatManager.Instance.ApplyElementalDamage(0, ELEMENTAL_TYPE.Fire, obj,
                    elementalTraitProcessor: (target, trait) => TraitManager.Instance.ProcessBurningTrait(target, trait, ref burningSource));
                bs = burningSource;
            } else {
                BurningSource burningSource = bs;
                obj.AdjustHP(processedDamage, ELEMENTAL_TYPE.Fire, true, elementalTraitProcessor: (target, trait) => TraitManager.Instance.ProcessBurningTrait(target, trait, ref burningSource), showHPBar: true, piercingPower: piercing, isPlayerSource: IsCastedByPlayer, source: IsCastedByPlayer ? brimstonesData : null);
                bs = burningSource;
            }
        } else if (traitable is Character character) {
            //int processedDamage = m_brimstoneBaseDamage - (m_brimstoneBaseDamage * PlayerSkillManager.Instance.GetAdditionalDamageBaseOnLevel(PLAYER_SKILL_TYPE.BRIMSTONES));
            BurningSource burningSource = bs;
            character.AdjustHP(processedDamage, ELEMENTAL_TYPE.Fire, true, 
                elementalTraitProcessor: (target, trait) => TraitManager.Instance.ProcessBurningTrait(target, trait, ref burningSource), showHPBar: true, piercingPower: piercing, isPlayerSource: IsCastedByPlayer, source: IsCastedByPlayer ? brimstonesData : null);
            bs = burningSource;
            Messenger.Broadcast(PlayerSignals.PLAYER_HIT_CHARACTER_VIA_SPELL, character, processedDamage);
            if (character.isDead && character.skillCauseOfDeath == PLAYER_SKILL_TYPE.NONE) {
                character.skillCauseOfDeath = PLAYER_SKILL_TYPE.BRIMSTONES;
                //Messenger.Broadcast(PlayerSignals.CREATE_SPIRIT_ENERGY, character.deathTilePosition.centeredWorldLocation, 1, character.deathTilePosition.parentMap);
                //Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, character.deathTilePosition.centeredWorldLocation, 1, character.deathTilePosition.parentMap);
            }
            if (Random.Range(0, 100) < 25) {
                character.traitContainer.AddTrait(character, "Injured");
            }
        } else {
            BurningSource burningSource = bs;
            //int processedDamage = m_brimstoneBaseDamage - (m_brimstoneBaseDamage * PlayerSkillManager.Instance.GetAdditionalDamageBaseOnLevel(PLAYER_SKILL_TYPE.BRIMSTONES));
            traitable.AdjustHP(processedDamage, ELEMENTAL_TYPE.Fire, true, 
                elementalTraitProcessor: (target, trait) => TraitManager.Instance.ProcessBurningTrait(target, trait, ref burningSource), showHPBar: true, piercingPower: piercing, isPlayerSource: IsCastedByPlayer, source: IsCastedByPlayer ? brimstonesData : null);
            bs = burningSource;
        }
    }
    public override void Reset() {
        base.Reset();
        IsCastedByPlayer = false;
    }
}
