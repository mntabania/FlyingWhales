using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inner_Maps;
using EZObjectPools;

public class BaseParticleEffect : PooledObject {

    public static System.Action<BaseParticleEffect> particleEffectActivated;
    public static System.Action<BaseParticleEffect> particleEffectDeactivated;
    
    public ParticleSystem[] particleSystems;
    private ParticleSystemRenderer[] _particleSystemRenderers;
    public bool pauseOnGamePaused;

    public LocationGridTile targetTile { get; protected set; }

    public void SetTargetTile(LocationGridTile tile) {
        targetTile = tile;
    }
    private void OnEnable() {
        // if (pauseOnGamePaused) {
        //     Messenger.AddListener<bool>(UISignals.PAUSED, OnGamePaused);
        // }
        particleEffectActivated?.Invoke(this);
    }
    private void OnDisable() {
        // if (pauseOnGamePaused) {
        //     Messenger.RemoveListener<bool>(UISignals.PAUSED, OnGamePaused);
        // }
        particleEffectDeactivated?.Invoke(this);
    }
    private void TryConstructParticleSystemRenderers() {
        if(_particleSystemRenderers == null || _particleSystemRenderers.Length <= 0) {
            _particleSystemRenderers = new ParticleSystemRenderer[particleSystems.Length];
            for (int i = 0; i < particleSystems.Length; i++) {
                _particleSystemRenderers[i] = particleSystems[i].GetComponent<ParticleSystemRenderer>();
            }
        }
    }
    public void PlayParticleEffect() {
        StartCoroutine(PlayParticleCoroutine());
    }
    public void StopParticleEffect() {
        StopParticle();
    }
    public void ResetParticleEffect() {
        ResetParticle();
    }
    public void SetSortingOrder(int amount) {
        TryConstructParticleSystemRenderers();
        for (int i = 0; i < _particleSystemRenderers.Length; i++) {
            _particleSystemRenderers[i].sortingOrder = amount;
        }
    }
    protected virtual IEnumerator PlayParticleCoroutine() {
        //Playing particle effect is done in a coroutine so that it will wait one frame before pausing the particles if the game is paused when the particle is activated
        //This will make sure that the particle effect will show but it will be paused right away
        PlayParticle();
        yield return null;
        if (pauseOnGamePaused && GameManager.Instance.isPaused) {
            PauseParticle();
        }
    }
    protected virtual void PlayParticle() {
        for (int i = 0; i < particleSystems.Length; i++) {
            particleSystems[i].Play();
        }
    }
    protected virtual void PauseParticle() {
        for (int i = 0; i < particleSystems.Length; i++) {
            particleSystems[i].Pause();
        }
    }
    protected virtual void StopParticle() {
        for (int i = 0; i < particleSystems.Length; i++) {
            particleSystems[i].Stop();
        }
    }
    protected virtual void ResetParticle() {
        for (int i = 0; i < particleSystems.Length; i++) {
            ParticleSystem ps = particleSystems[i];
            ps.Clear();
            ps.Stop();
        }
    }
    public void OnGamePaused(bool state) {
        if (state) {
            for (int i = 0; i < particleSystems.Length; i++) {
                if (particleSystems[i].isPlaying) {
                    particleSystems[i].Pause();
                }
            }
        } else {
            for (int i = 0; i < particleSystems.Length; i++) {
                if (particleSystems[i].isPaused) {
                    particleSystems[i].Play();
                }
            }
        }
    }
    public virtual void SetSize(Vector2Int p_size) {
        for (int i = 0; i < particleSystems.Length; i++) {
            ParticleSystem currentParticle = particleSystems[i];
            ParticleSystem.ShapeModule shapeModule = currentParticle.shape; 
            shapeModule.scale = new Vector3(p_size.x, p_size.y, 1f);
        }
    }

    #region Object Pool
    public override void Reset() {
        base.Reset();
        ResetParticleEffect();
        targetTile = null;
    }
    #endregion
}
