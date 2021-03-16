using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class TimerItemUI : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI lblTimerName;
    [SerializeField] private Slider barProgress;

    private RuinarchTimer _timer;
    
    private void OnEnable() {
        Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
        ForceUpdateOnEnable();
    }
    private void OnDisable() {
        Messenger.RemoveListener(Signals.TICK_ENDED, OnTickEnded);
    }
    private void OnTickEnded() {
        if (_timer == null) { return; }
        SetCurrentProgress(_timer.GetCurrentTimerProgressPercent());
    }
    public void SetTimer(RuinarchTimer p_timer) {
        _timer = p_timer;
    }
    public void SetName(string p_name) {
        lblTimerName.text = p_name;
    }
    private void SetCurrentProgress(float p_progress) {
        barProgress.DOValue(p_progress, 0.1f);
        // barProgress.value = p_progress;
    }
    private void ForceUpdateOnEnable() {
        barProgress.value = _timer?.GetCurrentTimerProgressPercent() ?? 0f;
    }
    private void OnDestroy() {
        _timer = null;
    }
}
