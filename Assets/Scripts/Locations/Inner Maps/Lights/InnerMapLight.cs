using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Experimental.Rendering.Universal;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Light2D))]
public class InnerMapLight : MonoBehaviour{

    [SerializeField] private Light2D _light;
    [SerializeField] private float _brightestIntensity;
    [SerializeField] private float _darkestIntensity;

    private GameObject parent;

    private float randomOnValue;
    private void Awake() {
        randomOnValue = 0.95f;  //UnityEngine.Random.Range(0.9f, 0.95f);
    }
    private void OnEnable() {
        InstantUpdateLightBasedOnGlobalLight(LightingManager.Instance.isTransitioning ? LightingManager.Instance.transitioningTo : LightingManager.Instance.currentGlobalLightState);
        Messenger.AddListener<LightingManager.Light_State>(InnerMapSignals.UPDATE_INNER_MAP_LIGHT, UpdateLightBasedOnGlobalLight);
        Messenger.AddListener<LightingManager.Light_State>(InnerMapSignals.INSTANT_UPDATE_INNER_MAP_LIGHT, InstantUpdateLightBasedOnGlobalLight);
    }
    private void OnDisable() {
        Messenger.RemoveListener<LightingManager.Light_State>(InnerMapSignals.UPDATE_INNER_MAP_LIGHT, UpdateLightBasedOnGlobalLight);
        Messenger.RemoveListener<LightingManager.Light_State>(InnerMapSignals.INSTANT_UPDATE_INNER_MAP_LIGHT, InstantUpdateLightBasedOnGlobalLight);
    }
    private void UpdateLightBasedOnGlobalLight(LightingManager.Light_State globalLightState) {
        //set intensity as inverse of given light state.
        var targetIntensity = GetTargetIntensity(globalLightState);
        DOTween.To(SetLightIntensity, _light.intensity, targetIntensity, 8f);
    }
    private void InstantUpdateLightBasedOnGlobalLight(LightingManager.Light_State globalLightState) {
        SetLightIntensity(GetTargetIntensity(globalLightState));
    }
    private float GetTargetIntensity(LightingManager.Light_State lightState) {
        return lightState == LightingManager.Light_State.Bright ? _darkestIntensity : _brightestIntensity;
    }
    private void SetLightIntensity(float intensity) {
        _light.intensity = intensity;
        //only enable light if intensity is greater than X, because of alpha overlap option
        //alpha overlap option was toggled on to prevent additive brightness when lights overlap.
        // _light.blendStyleIndex = intensity >= randomOnValue ? 0 : 1;
        if (_light.alphaBlendOnOverlap) {
            _light.enabled = intensity >= randomOnValue;    
        } else {
            _light.enabled = true;
        }
    }
}
