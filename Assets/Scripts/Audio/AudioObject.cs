using System;
using System.Collections;
using System.Collections.Generic;
using EZObjectPools;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioObject : PooledObject {

    [SerializeField] private AudioSource audioSource;

    public void Initialize(AudioClip audioClip, int tileRange, bool loop) {
        audioSource.clip = audioClip;
        audioSource.loop = loop;
        audioSource.minDistance = tileRange;
        audioSource.maxDistance = tileRange;
        audioSource.Stop();
        audioSource.Play();
    }
    private void LateUpdate() {
        if (audioSource.loop == false) {
            if (audioSource.isPlaying == false) {
                //if audio source is not looping and is not playing, destroy this
                ObjectPoolManager.Instance.DestroyObject(this);
            }
        }
    }
    public override void Reset() {
        base.Reset();
        audioSource.Stop();
    }
}
