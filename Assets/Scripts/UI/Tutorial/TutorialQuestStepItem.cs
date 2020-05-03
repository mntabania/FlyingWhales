using DG.Tweening;
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
    [SerializeField] private RectTransform _container;
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
    public void TweenIn() {
        _container.anchoredPosition = new Vector2(450f, 0f);
        _container.DOAnchorPosX(0f, 0.4f);
    }
    public void TweenOut(System.Action onCompleteAction = null) {
        Tweener tween = _container.DOAnchorPosX(450f, 0.4f);
        if (onCompleteAction != null) {
            tween.OnComplete(onCompleteAction.Invoke);
        }
    }

    #region Hover
    public void ShowTooltip() {
        _step.onHoverOverAction?.Invoke(this);
    }
    public void HideTooltip() {
        _step.onHoverOutAction?.Invoke();
    }
    #endregion

    #region Object Pool
    public override void Reset() {
        base.Reset();
        _container.anchoredPosition = Vector2.zero;
        Messenger.RemoveListener<TutorialQuestStep>(Signals.TUTORIAL_STEP_COMPLETED, OnTutorialStepCompleted);
    }
    #endregion
    
    
}
