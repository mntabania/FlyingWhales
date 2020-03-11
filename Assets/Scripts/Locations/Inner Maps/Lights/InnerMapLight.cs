using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Experimental.Rendering.Universal;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Light2D))]
public class InnerMapLight : MonoBehaviour{

    [SerializeField] private Light2D _light;
    [SerializeField] private TimeOfDayLightDictionary _timeOfDayLightDictionary;
    private Tweener _flickerTweener;
    
    
    private void OnEnable() {
        Messenger.AddListener<TIME_IN_WORDS>(Signals.UPDATE_INNER_MAP_LIGHT, UpdateLightBasedOnTimeOfDay);
        UpdateLightBasedOnTimeOfDay(GameManager.GetCurrentTimeInWordsOfTick());
    }
    private void OnDisable() {
        Messenger.RemoveListener<TIME_IN_WORDS>(Signals.UPDATE_INNER_MAP_LIGHT, UpdateLightBasedOnTimeOfDay);
    }
    
    private void UpdateLightBasedOnTimeOfDay(TIME_IN_WORDS timeInWords) {
        Assert.IsTrue(_timeOfDayLightDictionary.ContainsKey(timeInWords), 
            $"There was no light setting for time of day {timeInWords.ToString()} for {name}");
        float targetLight = _timeOfDayLightDictionary[timeInWords];
        Tweener lightTween = DOTween.To(SetLightIntensity, _light.intensity, targetLight, 1f);
        // if (timeInWords == TIME_IN_WORDS.EARLY_NIGHT || timeInWords == TIME_IN_WORDS.LATE_NIGHT || timeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
        //     lightTween.OnComplete(StartFlicker);
        // } else {
        //     StopFlicker();
        // }
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
