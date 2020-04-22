using EZObjectPools;
using TMPro;
using Tutorial;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TutorialQuestStepItem : PooledObject {
    
    [SerializeField] private TextMeshProUGUI _stepLbl;
    [SerializeField] private Toggle _completedToggle;
    [SerializeField] private EventLabel _eventLabel;
    public UIHoverPosition hoverPosition;
    
    private TutorialQuestStep _step;

    public void SetStep(TutorialQuestStep step) {
        _step = step;
        _completedToggle.isOn = step.isCompleted;

        _stepLbl.text = _step.hasHoverAction ? $"<link=\"1\"><#CEB67C>{step.stepDescription}</color></link>" : step.stepDescription;
        
        //update hover actions based on whether or not the provided step has a tooltip.
        _eventLabel.enabled = step.hasHoverAction;
        _stepLbl.raycastTarget = step.hasHoverAction;
        
        Messenger.AddListener<TutorialQuestStep>(Signals.TUTORIAL_STEP_COMPLETED, OnTutorialStepCompleted);
    }
    private void OnTutorialStepCompleted(TutorialQuestStep step) {
        if (_step == step) {
            _completedToggle.isOn = true;
        }
    }
    public void ShowTooltip() {
        // UIManager.Instance.ShowSmallInfo(_step.tooltip);
        _step.onHoverOverAction?.Invoke(this);
    }
    public void HideTooltip() {
        // UIManager.Instance.HideSmallInfo();
        _step.onHoverOutAction?.Invoke();
    }
    public override void Reset() {
        base.Reset();
        Messenger.RemoveListener<TutorialQuestStep>(Signals.TUTORIAL_STEP_COMPLETED, OnTutorialStepCompleted);
    }
}
