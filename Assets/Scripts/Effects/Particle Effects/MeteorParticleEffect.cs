using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Traits;
using Inner_Maps;
using Scriptable_Object_Scripts;
using UtilityScripts;

public class MeteorParticleEffect : BaseParticleEffect {
    public ParticleSystem meteorParticle;
    private bool hasMeteorFell;

    private int m_baseDamage = -500;
    //public ParticleSystem[] meteorExplosionParticles;

    //private LocationGridTile targetTile;
    //private int radius;
    //public void MeteorStrike(LocationGridTile targetTile) { //, int radius
    //    this.targetTile = targetTile;
    //    //this.radius = radius;
    //    meteorParticle.Play();
    //}
    //protected override void PlayParticle() {
    //    base.PlayParticle();
    //    OnMeteorFell();
    //}
    //protected override void ParticleAfterEffect(ParticleSystem particleSystem) {
    //    OnMeteorFell();
    //}
    protected override IEnumerator PlayParticleCoroutine() {
        //Playing particle effect is done in a coroutine so that it will wait one frame before pausing the particles if the game is paused when the particle is activated
        //This will make sure that the particle effect will show but it will be paused right away
        PlayParticle();
        yield return new WaitForSeconds(0.1f);
        if ((pauseOnGamePaused && GameManager.Instance.isPaused) || !GameManager.Instance.gameHasStarted) {
            PauseParticle();
        }
    }
    protected override void ResetParticle() {
        base.ResetParticle();
        hasMeteorFell = false;
    }
    private void OnMeteorFell() {
        hasMeteorFell = true;
        AudioManager.Instance.TryCreateAudioObject(
            CollectionUtilities.GetRandomElement(PlayerSkillManager.Instance
                .GetPlayerSkillData<MeteorSkillData>(PLAYER_SKILL_TYPE.METEOR).impactSounds),
            targetTile, 3, false
        );
        //for (int i = 0; i < meteorExplosionParticles.Length; i++) {
        //    meteorExplosionParticles[i].Play();
        //}
        BurningSource bs = null;
        //List<LocationGridTile> tiles = targetTile.GetTilesInRadius(1, 0, true, true); //radius
        targetTile.PerformActionOnTraitables((traitable) => MeteorEffect(traitable, ref bs));
        for (int i = 0; i < targetTile.neighbourList.Count; i++) {
            LocationGridTile tile = targetTile.neighbourList[i];
            tile.PerformActionOnTraitables((traitable) => MeteorEffect(traitable, ref bs));
        }

        //Messenger.Broadcast(Signals.INCREASE_THREAT_THAT_SEES_TILE, targetTile, 10);
        targetTile.tileObjectComponent.genericTileObject.traitContainer.AddTrait(targetTile.tileObjectComponent.genericTileObject, "Danger Remnant");
        Messenger.Broadcast(SpellSignals.METEOR_FELL);
        InnerMapCameraMove.Instance.MeteorShake();
        targetTile.RemoveMeteor();
        //GameManager.Instance.StartCoroutine(ExpireCoroutine(gameObject));

    }
    private void MeteorEffect(ITraitable traitable, ref BurningSource bs) {
        if (traitable.gridTileLocation == null) { return; }
        BurningSource burningSource = bs;
        int processedDamage = m_baseDamage - PlayerSkillManager.Instance.GetAdditionalDamageBaseOnLevel(PLAYER_SKILL_TYPE.METEOR);
        traitable.AdjustHP(processedDamage, ELEMENTAL_TYPE.Fire, true, elementalTraitProcessor: (target, trait) => TraitManager.Instance.ProcessBurningTrait(target, trait, ref burningSource), showHPBar: true, piercingPower: PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.METEOR));
        //if (traitable is TileObject obj) {
        //    if (obj.tileObjectType != TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT) {
        //        obj.AdjustHP(-500, ELEMENTAL_TYPE.Fire, 
        //            elementalTraitProcessor: (target, trait) => TraitManager.Instance.ProcessBurningTrait(target, trait, ref burningSource), showHPBar: true);
        //    } else {
        //        CombatManager.Instance.ApplyElementalDamage(0, ELEMENTAL_TYPE.Fire, obj, 
        //            elementalTraitProcessor: (target, trait) => TraitManager.Instance.ProcessBurningTrait(target, trait, ref burningSource));
        //    }
        //} else if (traitable is Character character) {
        //    character.AdjustHP(-500, ELEMENTAL_TYPE.Fire, true, 
        //        elementalTraitProcessor: (target, trait) => TraitManager.Instance.ProcessBurningTrait(target, trait, ref burningSource), showHPBar: true);
        //} else {
        //    traitable.AdjustHP(-500, ELEMENTAL_TYPE.Fire, 
        //        elementalTraitProcessor: (target, trait) => TraitManager.Instance.ProcessBurningTrait(target, trait, ref burningSource), showHPBar: true);
        //}
        Character character = traitable as Character;
        if (character != null) {
            Messenger.Broadcast(PlayerSignals.PLAYER_HIT_CHARACTER_VIA_SPELL, character, processedDamage);
        }

        if (character != null && traitable.currentHP <= 0) {
            (character).skillCauseOfDeath = PLAYER_SKILL_TYPE.METEOR;
            Debug.LogError(character.marker + " -- " + character.currentRegion + " -- " + character.currentRegion.innerMap);
            Messenger.Broadcast(PlayerSignals.CREATE_SPIRIT_ENERGY, character.marker.transform.position, 1, character.currentRegion.innerMap);
        }
        bs = burningSource;
    }
    private void OnTweenComplete() {
        //InnerMapCameraMove.Instance.innerMapsCamera.transform.rotation = Quaternion.Euler(new Vector3(0f,0f,0f));
        InnerMapCameraMove.Instance.camera.transform.DORotate(new Vector3(0f, 0f, 0f), 0.2f);
    }
    private IEnumerator ExpireCoroutine(GameObject go) {
        yield return new WaitForSeconds(2f);
        ObjectPoolManager.Instance.DestroyObject(go);
    }

    private void Update() {
        if (!hasMeteorFell) {
            if (meteorParticle.isStopped) {
                OnMeteorFell();
            }
        } else {
            bool allInactive = true;
            for (int i = 0; i < particleSystems.Length; i++) {
                ParticleSystem currPS = particleSystems[i];
                if (currPS.IsAlive()) { //!currPS.isStopped
                    allInactive = false;
                    break;
                }
            }
            if (allInactive) {
                ObjectPoolManager.Instance.DestroyObject(gameObject);
            }
        }
    }
}
