using DG.Tweening;
using EZObjectPools;
using Quests;
using Quests.Steps;
using TMPro;
using Tutorial;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class QuestStepItem : PooledObject {
    
    [SerializeField] private TextMeshProUGUI _stepLbl;
    [SerializeField] private Toggle _completedToggle;
    [SerializeField] private EventLabel _eventLabel;
    [SerializeField] private RectTransform _container;
    [SerializeField] private Button centerButton;
    
    public UIHoverPosition hoverPosition;
    
    private QuestStep _step;

    public void SetStep(QuestStep step) {
        _step = step;
        _completedToggle.isOn = step.isCompleted;
        _stepLbl.text = _step.hasHoverAction ? $"<link=\"1\"><#CEB67C>{step.stepDescription}</color></link>" : step.stepDescription;
        
        //update hover actions based on whether or not the provided step has a tooltip.
        _eventLabel.enabled = step.hasHoverAction;
        _stepLbl.raycastTarget = step.hasHoverAction;
        
        //update center button based on the number of selectable objects the step has.
        centerButton.gameObject.SetActive(step.objectsToCenter != null && step.objectsToCenter.Count > 0);
        
        Messenger.AddListener<QuestStep>(Signals.QUEST_STEP_COMPLETED, OnStepCompleted);
    }
    private void OnStepCompleted(QuestStep step) {
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

    #region Center
    public void OnClickCenter() {
        _step.CenterCycle();
    }
    #endregion
    
    #region Object Pool
    public override void Reset() {
        base.Reset();
        _container.anchoredPosition = Vector2.zero;
        Messenger.RemoveListener<QuestStep>(Signals.QUEST_STEP_COMPLETED, OnStepCompleted);
    }
    #endregion
    
    
}
