using System.Collections;
using System.Collections.Generic;
using EZObjectPools;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SpellAudioObject : PooledObject {

    [SerializeField] private AudioSource spellAudio;

    public void Initialize(AudioClip audioClip, int tileRange, bool loop) {
        spellAudio.clip = audioClip;
        spellAudio.loop = loop;
        spellAudio.minDistance = tileRange;
        spellAudio.maxDistance = tileRange;
        spellAudio.Stop();
        spellAudio.Play();   
    }
    public override void Reset() {
        base.Reset();
        spellAudio.Stop();
    }
}
