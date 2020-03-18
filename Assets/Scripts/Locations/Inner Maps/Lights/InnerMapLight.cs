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
    private Tweener _flickerTweener;
    
    
    private void OnEnable() {
        Messenger.AddListener<LightingManager.Light_State>(Signals.UPDATE_INNER_MAP_LIGHT, UpdateLightBasedOnTimeOfDay);
        UpdateLightBasedOnTimeOfDay(LightingManager.Instance.currentLightState);
    }
    private void OnDisable() {
        Messenger.RemoveListener<LightingManager.Light_State>(Signals.UPDATE_INNER_MAP_LIGHT, UpdateLightBasedOnTimeOfDay);
    }
    
    private void UpdateLightBasedOnTimeOfDay(LightingManager.Light_State lightState) {
        //set intensity as inverse of given light state.
        var targetIntensity = lightState == LightingManager.Light_State.Bright ? _darkestIntensity : _brightestIntensity;
        DOTween.To(SetLightIntensity, _light.intensity, targetIntensity, 1f);
    }
    private void StartFlicker() {
        _flickerTweener = DOTween.To(SetLightIntensity, _light.intensity, Random.Range(_light.intensity - 0.4f, _light.intensity + 0.4f),
            Random.Range(0.3f, 0.5f)).SetLoops(-1);
    }
    private void StopFlicker() {
        _flickerTweener?.Kill();
    }
    private void SetLightIntensity(float intensity) {
        _light.intensity = intensity;
    }
    
    /*
     * 
        
        switch (timeInWords) {
            case TIME_IN_WORDS.MORNING:
            case TIME_IN_WORDS.AFTERNOON:
            case TIME_IN_WORDS.LUNCH_TIME:
                targetLight = 0f;
                break;
            case TIME_IN_WORDS.EARLY_NIGHT:
                targetLight = 0.5f;
                break;
            case TIME_IN_WORDS.LATE_NIGHT:
            case TIME_IN_WORDS.AFTER_MIDNIGHT:
                targetLight = 0.7f;
                break;
            default:
                throw new System.Exception(");
        }
     */
}
