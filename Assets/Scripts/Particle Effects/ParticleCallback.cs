using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach this to any game object that has a Particle System attached to it,
/// this will execute any action that you have assigned after the particle system has stopped.
/// </summary>
public class ParticleCallback : MonoBehaviour {

    private System.Action _onParticleStopped;

    public void SetAction(System.Action onParticleStopped) {
        _onParticleStopped = onParticleStopped;
    }
    
    private void OnParticleSystemStopped() {
        _onParticleStopped?.Invoke();
    }
}
