using DG.Tweening;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class LightingManager : MonoBehaviour {
    public static LightingManager Instance;

    [Header("Lights")] 
    [SerializeField] private Light2D _globalLight;
    [SerializeField] private float _brightestIntensity;
    [SerializeField] private float _darkestIntensity;

    [Header("Transitions")]
    [SerializeField] private IntRange _darkPeriodRange;
    [SerializeField] private IntRange _brightPeriodRange;
    
    //what time of day is the light currently for, this is so that the light does not need to change
    //if it is already at the current time of day setting.
    private TIME_IN_WORDS _currentTimeLight = TIME_IN_WORDS.NONE;
    
    private int _darkToLightTickDifference;
    private int _lightToDarkTickDifference;

    public enum Light_State { Dark, Bright }
    public Light_State currentGlobalLightState = Light_State.Bright;
    [SerializeField] private bool _isTransitioning;
    [SerializeField] private Light_State _transitioningTo;
    
    private Tweener _currentTween;
    
    #region getters
    public bool isTransitioning => _isTransitioning;
    public Light_State transitioningTo => _transitioningTo;
    #endregion
    
    private void Awake() {
        Instance = this;
    }

    public void Initialize() {
        Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
        Messenger.AddListener<bool>(Signals.PAUSED, OnGamePaused);
        Messenger.AddListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        ComputeLightingValues();
        SetGlobalLightIntensity(_brightestIntensity);
    }
    private void OnTickEnded() {
        UpdateAllLightsBasedOnTimeOfDay(GameManager.Instance.Today());
    }
    private void ComputeLightingValues() {
        int darkToLightDifferenceInHours = Mathf.Abs(_darkPeriodRange.lowerBound - _brightPeriodRange.lowerBound);
        _darkToLightTickDifference = GameManager.Instance.GetTicksBasedOnHour(darkToLightDifferenceInHours);

        int lightToDarkDifferenceInHours = Mathf.Abs(_darkPeriodRange.upperBound - _brightPeriodRange.upperBound);
        _lightToDarkTickDifference = GameManager.Instance.GetTicksBasedOnHour(lightToDarkDifferenceInHours);
    }
    private void UpdateAllLightsBasedOnTimeOfDay(GameDate date) {
        if (_darkPeriodRange.IsOutsideRange(GameManager.Instance.GetCeilingHoursBasedOnTicks(date.tick))) {
            SetCurrentLightState(Light_State.Dark);
        } else if (_brightPeriodRange.IsInRange(GameManager.Instance.GetCeilingHoursBasedOnTicks(date.tick))) {
            SetCurrentLightState(Light_State.Bright);
        }
        else {
            if (isTransitioning) { return; }
            _isTransitioning = true;
            //transitioning
            Light_State targetLightState =
                currentGlobalLightState == Light_State.Dark ? Light_State.Bright : Light_State.Dark;
            _transitioningTo = targetLightState;
            Messenger.Broadcast(Signals.UPDATE_INNER_MAP_LIGHT, targetLightState); //update other lights based on target light state
            var targetIntensity = currentGlobalLightState == Light_State.Dark ? _brightestIntensity : _darkestIntensity;
            _currentTween = DOTween.To(SetGlobalLightIntensity, _globalLight.intensity, targetIntensity, 
                _darkToLightTickDifference * GameManager.Instance.GetTickSpeed(PROGRESSION_SPEED.X1)).OnComplete(OnDoneTransition);
            OnProgressionSpeedChanged(GameManager.Instance.currProgressionSpeed);
        }
    }
    private void OnDoneTransition() {
        _isTransitioning = false;
        _currentTween = null;
    }
    private void OnGamePaused(bool isPaused) {
        if (isPaused) {
            _currentTween?.Pause();
        } else {
            _currentTween?.Play();
        }
    }
    private void OnProgressionSpeedChanged(PROGRESSION_SPEED progression) {
        if (_currentTween == null) { return; }
        switch (progression) {
            case PROGRESSION_SPEED.X1:
                _currentTween.timeScale = 1f;
                break;
            case PROGRESSION_SPEED.X2:
                _currentTween.timeScale = 1.2f;
                break;
            case PROGRESSION_SPEED.X4:
                _currentTween.timeScale = 1.4f;
                break;
        }
    }
    private void SetGlobalLightIntensity(float intensity) {
        _globalLight.intensity = intensity;
    }
    private void SetCurrentLightState(Light_State lightState) {
        if (currentGlobalLightState == lightState) { return; }
        currentGlobalLightState = lightState;
    }
    
}
