using System;
using System.Collections;
using System.Collections.Generic;
using EZObjectPools;
using UnityEngine;

public class AutoDestroyParticleOnDisable : PooledObject {
    private void OnParticleSystemStopped() {
        ObjectPoolManager.Instance.DestroyObject(this);
    }
    // private void OnDisable() {
    //     ObjectPoolManager.Instance.DestroyObject(this);
    // }
}
