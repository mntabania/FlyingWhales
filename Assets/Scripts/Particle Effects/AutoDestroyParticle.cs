using System;
using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroyParticle : PooledObject {

    [SerializeField] private ParticleSystem[] particleSystems;
    private void OnEnable() {
        for (int i = 0; i < particleSystems.Length; i++) {
            ParticleSystem ps = particleSystems[i];
            ps.Stop();
            ps.Clear();
            ps.Play();
        }
    }
    private void LateUpdate() {
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

    public void StopEmission() {
        for (int i = 0; i < particleSystems.Length; i++) {
            ParticleSystem ps = particleSystems[i];
            ps.Stop();
            ps.Clear();
        }
    }
    
    public override void Reset() {
        base.Reset();
        for (int i = 0; i < particleSystems.Length; i++) {
            ParticleSystem ps = particleSystems[i];
            ps.Stop();
            ps.Clear();
        }
    }

}
