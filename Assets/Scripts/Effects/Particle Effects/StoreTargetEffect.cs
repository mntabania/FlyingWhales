using System;
using EZObjectPools;
using UnityEngine;

public class StoreTargetEffect : PooledObject {
    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private GameObject goImage;

    public ParticleSystem trailParticles => _particleSystem;
    private void OnEnable() {
        _particleSystem.Play();
    }
    private void LateUpdate() {
        if (!_particleSystem.isEmitting && _particleSystem.particleCount <= 0) {
            ObjectPoolManager.Instance.DestroyObject(gameObject);
        }
    }
    public void SetImageState(bool p_state) {
        goImage.SetActive(p_state);
    }
    public override void Reset() {
        base.Reset();
        _particleSystem.Clear();
        SetImageState(true);
    }
}
