using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inner_Maps;
using EZObjectPools;

public class BaseParticleEffect : PooledObject {
    public ParticleSystem[] particleSystems;
    public LocationGridTile targetTile { get; protected set; }

    public void SetTargetTile(LocationGridTile tile) {
        targetTile = tile;
    }

    private void OnEnable() {
        Messenger.AddListener<ParticleSystem>(Signals.PARTICLE_EFFECT_DONE, OnParticleEffectDonePlaying);
    }
    private void OnDisable() {
        if (Messenger.eventTable.ContainsKey(Signals.PARTICLE_EFFECT_DONE)) {
            Messenger.RemoveListener<ParticleSystem>(Signals.PARTICLE_EFFECT_DONE, OnParticleEffectDonePlaying);
        }
    }
    private void OnParticleEffectDonePlaying(ParticleSystem particleSystem) {
        if (particleSystems.Contains(particleSystem)) {
            Messenger.RemoveListener<ParticleSystem>(Signals.PARTICLE_EFFECT_DONE, OnParticleEffectDonePlaying);
            ParticleAfterEffect(particleSystem);
        }
    }
    public virtual void PlayParticleEffect() {
        for (int i = 0; i < particleSystems.Length; i++) {
            particleSystems[i].Play();
        }
    }
    public virtual void StopParticleEffect() {
        for (int i = 0; i < particleSystems.Length; i++) {
            particleSystems[i].Stop();
        }
    }
    public virtual void ResetParticleEffect() {
        for (int i = 0; i < particleSystems.Length; i++) {
            particleSystems[i].Clear();
        }
    }
    protected virtual void ParticleAfterEffect(ParticleSystem particleSystem) {
    }

    #region Object Pool
    public override void Reset() {
        base.Reset();
        ResetParticleEffect();
        targetTile = null;
    }
    #endregion
}
