using System;
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
    [SerializeField] private Image _toggleImage;
    [SerializeField] private EventLabel _eventLabel;
    [SerializeField] private RectTransform _container;
    [SerializeField] private Button centerButton;

    [SerializeField] private Sprite checkSprite;
    [SerializeField] private Sprite crossSprite;
    
    public UIHoverPosition hoverPosition;
    
    private QuestStep _step;

    public void SetStep(QuestStep step) {
        _step = step;
        _completedToggle.isOn = step.isCompleted;
        UpdateDescription();
        
        //update hover actions based on whether or not the provided step has a tooltip.
        _eventLabel.enabled = step.hasHoverAction;
        _stepLbl.raycastTarget = step.hasHoverAction;
        
        //update center button based on the number of selectable objects the step has.
        centerButton.gameObject.SetActive(step.HasObjectsToCenter());
        
        Messenger.AddListener<QuestStep>(Signals.QUEST_STEP_COMPLETED, OnStepCompleted);
        Messenger.AddListener<QuestStep>(Signals.QUEST_STEP_FAILED, OnStepFailed);
        Messenger.AddListener<QuestStep>(Signals.UPDATE_QUEST_STEP_ITEM, UpdateInfo);
    }
    private void UpdateInfo(QuestStep step) {
        if (_step == step) {
            UpdateDescription();
        }
    }
    private void UpdateDescription() {
        _stepLbl.text = _step.hasHoverAction ? $"<link=\"1\"><#CEB67C>{_step.stepDescription}</color></link>" : _step.stepDescription;
    }
    private void OnStepCompleted(QuestStep step) {
        if (_step == step) {
            _completedToggle.isOn = true;
            _toggleImage.sprite = checkSprite;
            _toggleImage.rectTransform.DOPunchScale(new Vector3(2f, 2f, 2f), 0.2f);
        }
    }
    private void OnStepFailed(QuestStep step) {
        if (_step == step) {
            _completedToggle.isOn = true;
            _toggleImage.sprite = crossSprite;
            _toggleImage.rectTransform.DOPunchScale(new Vector3(2f, 2f, 2f), 0.2f);
        }
    }

    #region Tweening
    public void TweenIn() {
        _container.anchoredPosition = new Vector2(450f, 0f);
        _container.DOAnchorPosX(0f, 0.4f);
    }
    public void TweenOut(Action onCompleteAction = null) {
        Tweener tween = _container.DOAnchorPosX(450f, 0.4f);
        if (onCompleteAction != null) {
            tween.OnComplete(onCompleteAction.Invoke);
        }
    }
    #endregion

    #region Hover
    public void ShowTooltip() {
        _step.ExecuteHoverAction(this);
    }
    public void HideTooltip() {
        _step.ExecuteHoverOutAction();
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
        Messenger.RemoveListener<QuestStep>(Signals.QUEST_STEP_FAILED, OnStepFailed);
        Messenger.RemoveListener<QuestStep>(Signals.UPDATE_QUEST_STEP_ITEM, UpdateInfo);
    }
    #endregion
}
