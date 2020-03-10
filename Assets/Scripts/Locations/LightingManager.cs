using DG.Tweening;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class LightingManager : MonoBehaviour {
    public static LightingManager Instance;

    [Header("Lights")] 
    [SerializeField] private Light2D _globalLight;

    //what time of day is the light currently for, this is so that the light does not need to change
    //if it is already at the current time of day setting.
    private TIME_IN_WORDS _currentTimeLight = TIME_IN_WORDS.NONE; 
    
    private void Awake() {
        Instance = this;
    }

    public void Initialize() {
        Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
        UpdateAllLightsBasedOnTimeOfDay(GameManager.GetCurrentTimeInWordsOfTick());
    }
    private void OnTickEnded() {
        UpdateAllLightsBasedOnTimeOfDay(GameManager.GetCurrentTimeInWordsOfTick());
    }
    
    private void UpdateAllLightsBasedOnTimeOfDay(TIME_IN_WORDS timeInWords) {
        if (_currentTimeLight == timeInWords) { return; } //ignore change
        float targetLight;
        switch (timeInWords) {
            case TIME_IN_WORDS.MORNING:
                targetLight = 0.7f;
                break;
            case TIME_IN_WORDS.AFTERNOON:
            case TIME_IN_WORDS.LUNCH_TIME:
                targetLight = 1f;
                break;
            case TIME_IN_WORDS.EARLY_NIGHT:
                targetLight = 0.4f;
                break;
            case TIME_IN_WORDS.LATE_NIGHT:
                targetLight = 0.2f;
                break;
            case TIME_IN_WORDS.AFTER_MIDNIGHT:
                targetLight = 0.4f;
                break;
            default:
                throw new System.Exception($"There was no light setting for time of day {timeInWords.ToString()}");
        }
        _currentTimeLight = timeInWords;
        DOTween.To(SetGlobalLightIntensity, _globalLight.intensity, targetLight, 1f);
        Messenger.Broadcast(Signals.UPDATE_INNER_MAP_LIGHT, timeInWords);
    }
    private void SetGlobalLightIntensity(float intensity) {
        _globalLight.intensity = intensity;
    }
    
    
}
