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
    
    private void OnEnable() {
        InstantUpdateLightBasedOnGlobalLight(LightingManager.Instance.currentLightState);
        Messenger.AddListener<LightingManager.Light_State>(Signals.UPDATE_INNER_MAP_LIGHT, UpdateLightBasedOnGlobalLight);
    }
    private void OnDisable() {
        Messenger.RemoveListener<LightingManager.Light_State>(Signals.UPDATE_INNER_MAP_LIGHT, UpdateLightBasedOnGlobalLight);
    }
    private void UpdateLightBasedOnGlobalLight(LightingManager.Light_State globalLightState) {
        //set intensity as inverse of given light state.
        var targetIntensity = GetTargetIntensity(globalLightState);
        DOTween.To(SetLightIntensity, _light.intensity, targetIntensity, 1f);
    }
    private void InstantUpdateLightBasedOnGlobalLight(LightingManager.Light_State globalLightState) {
        SetLightIntensity(GetTargetIntensity(globalLightState));
    }
    private float GetTargetIntensity(LightingManager.Light_State lightState) {
        return lightState == LightingManager.Light_State.Bright ? _darkestIntensity : _brightestIntensity;
    }
    private void SetLightIntensity(float intensity) {
        _light.intensity = intensity;
    }
}
