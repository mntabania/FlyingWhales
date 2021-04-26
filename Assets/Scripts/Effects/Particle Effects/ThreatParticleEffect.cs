using System;
using System.Collections;
using System.Collections.Generic;
using Quests;
using UnityEngine;

public class ThreatParticleEffect : MonoBehaviour {
    public ParticleSystem[] threatParticleSystems;
    public GameObject leftParticleGO;
    public GameObject rightParticleGO;
    public GameObject bottomParticleGO;
    private bool _isPlaying;
    
    private void Start() {
        Messenger.AddListener(PlayerSignals.START_THREAT_EFFECT, OnStartThreatEffect);
        Messenger.AddListener(PlayerSignals.STOP_THREAT_EFFECT, OnStopThreatEffect);
        Messenger.AddListener<Region>(RegionSignals.REGION_MAP_OPENED, OnInnerMapOpened);
        Messenger.AddListener<Region>(RegionSignals.REGION_MAP_CLOSED, OnInnerMapClosed);
        Messenger.AddListener<Quest>(PlayerQuestSignals.QUEST_ACTIVATED, OnQuestActivated);
        Messenger.AddListener<Quest>(PlayerQuestSignals.QUEST_DEACTIVATED, OnQuestDeactivated);
        //Messenger.AddListener<Camera>(Signals.ZOOM_INNER_MAP_CAMERA, OnZoomInnerMapCamera);
        //Messenger.AddListener<Camera>(Signals.ZOOM_WORLD_MAP_CAMERA, OnZoomWorldMapCamera);
    }
    private void OnDestroy() {
        Messenger.RemoveListener(PlayerSignals.START_THREAT_EFFECT, OnStartThreatEffect);
        Messenger.RemoveListener(PlayerSignals.STOP_THREAT_EFFECT, OnStopThreatEffect);
    }
    private void OnInnerMapOpened(Region region) {
        gameObject.transform.SetParent(InnerMapCameraMove.Instance.transform);
        gameObject.transform.localPosition = Vector3.zero;
        UpdatePosition(InnerMapCameraMove.Instance.camera);
        if (_isPlaying) {
            StopEffect();
            PlayEffect();
        }
    }
    private void OnInnerMapClosed(Region region) {
        gameObject.transform.localPosition = Vector3.zero;
        if (_isPlaying) {
            StopEffect();
            PlayEffect();
        }
    }
    public void OnZoomCamera(Camera camera) {
        UpdatePosition(camera);
    }
    private void UpdatePosition(Camera camera) {
        //float pos = camera.orthographicSize * 2f;
        //float leftXPos = pos * -1f;
        //float rightXPos = pos;

        Vector3 leftViewPoint = camera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        Vector3 rightViewPoint = camera.ViewportToWorldPoint(new Vector3(1f, 0f, 0f));
        Vector3 bottomViewPoint = camera.ViewportToWorldPoint(new Vector3(0.5f, 0f, 0f));

        leftParticleGO.transform.position =  new Vector3((leftViewPoint.x - 1f), leftParticleGO.transform.position.y, leftParticleGO.transform.position.z);
        rightParticleGO.transform.position = new Vector3((rightViewPoint.x + 1f), rightParticleGO.transform.position.y, rightParticleGO.transform.position.z);
        bottomParticleGO.transform.position = new Vector3(bottomParticleGO.transform.position.x, (bottomViewPoint.y - 1f), bottomParticleGO.transform.position.z);
    }

    #region Listeners
    private void OnStartThreatEffect() {
        CheckEffectState();
    }
    private void OnStopThreatEffect() {
        CheckEffectState();
    }
    private void OnQuestActivated(Quest quest) {
        if (quest is DivineIntervention) {
            CheckEffectState();
        }
    }
    private void OnQuestDeactivated(Quest quest) {
        if (quest is DivineIntervention) {
            CheckEffectState();
        }
    }
    #endregion
    
    private void CheckEffectState() {
        if (QuestManager.Instance.IsQuestActive<DivineIntervention>() 
            || PlayerManager.Instance.player.retaliationComponent.isRetaliating) { //|| PlayerManager.Instance.player.threatComponent.threat >= ThreatComponent.MAX_THREAT
            //play effect if threat is at max or counterattack quest is active or divine intervention quest is active
            PlayEffect();
        } else {
            StopEffect();
        }
    }
    
    private void PlayEffect() {
        if (_isPlaying) { return; } //effect is already playing
        _isPlaying = true;
        for (int i = 0; i < threatParticleSystems.Length; i++) {
            var threatParticleSystem = threatParticleSystems[i];
            if (!threatParticleSystem.isPlaying) {
                threatParticleSystem.Play();    
            }
        }
    }
    private void StopEffect() {
        if (_isPlaying == false) { return; } //effect isn't playing
        _isPlaying = false;
        for (int i = 0; i < threatParticleSystems.Length; i++) {
            var threatParticleSystem = threatParticleSystems[i];
            if (threatParticleSystem.isPlaying) {
                threatParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);    
            }
        }
    }
}
