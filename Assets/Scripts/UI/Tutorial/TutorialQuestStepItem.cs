using EZObjectPools;
using TMPro;
using Tutorial;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TutorialQuestStepItem : PooledObject {
    
    [SerializeField] private TextMeshProUGUI _stepLbl;
    [SerializeField] private Toggle _completedToggle;
    [SerializeField] private HoverHandler _hoverHandler;
    
    private TutorialQuestStep _step;

    public void SetStep(TutorialQuestStep step) {
        _step = step;
        _stepLbl.text = step.stepDescription;
        _completedToggle.isOn = step.isCompleted;
        
        //update hover actions based on whether or not the provided step has a tooltip.
        bool hasTooltip = string.IsNullOrEmpty(step.tooltip) == false;
        _hoverHandler.enabled = hasTooltip;
        _stepLbl.raycastTarget = hasTooltip;
        
        Messenger.AddListener<TutorialQuestStep>(Signals.TUTORIAL_STEP_COMPLETED, OnTutorialStepCompleted);
    }
    private void OnTutorialStepCompleted(TutorialQuestStep step) {
        if (_step == step) {
            _completedToggle.isOn = true;
        }
    }
    public void ShowTooltip() {
        UIManager.Instance.ShowSmallInfo(_step.tooltip);
    }
    public void HideTooltip() {
        UIManager.Instance.HideSmallInfo();
    }
    public override void Reset() {
        base.Reset();
        Messenger.RemoveListener<TutorialQuestStep>(Signals.TUTORIAL_STEP_COMPLETED, OnTutorialStepCompleted);
    }
}
