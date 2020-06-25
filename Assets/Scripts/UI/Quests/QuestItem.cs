using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using EZObjectPools;
using Quests;
using Quests.Steps;
using TMPro;
using Tutorial;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UtilityScripts;

public class QuestItem : PooledObject {

    [SerializeField] private RuinarchText headerLbl;
    [SerializeField] private Transform stepsParent;
    [FormerlySerializedAs("tutorialQuestStepPrefab")] [SerializeField] private GameObject questStepPrefab;
    public ContentSizeFitter contentSizeFitter;
    private Vector2 _defaultSize;
    private Quest _quest;
    
    private void Awake() {
        _defaultSize = (transform as RectTransform).sizeDelta;
    }
    
    public void SetQuest(Quest quest) {
        _quest = quest;
        headerLbl.SetText(quest.questName);
        UpdateSteps();
    }

    #region Tweening
    public void TweenIn() {
        RectTransform rectTransform = transform as RectTransform;
        Vector2 targetSize = new Vector2(392f, 55f);
        rectTransform.sizeDelta = new Vector2(0f, rectTransform.sizeDelta.y);
        rectTransform.DOSizeDelta(targetSize, 0.4f).SetEase(Ease.InCubic);
    }
    public void TweenOutDelayed() {
        StartCoroutine(HideTutorialQuestCoroutine(_quest));
    }
    private IEnumerator HideTutorialQuestCoroutine(Quest quest) {
        yield return new WaitForSeconds(1.5f);
        RectTransform rectTransform = quest.questItem.transform as RectTransform;
        Vector2 targetSize = rectTransform.sizeDelta;
        targetSize.x = 0f;
        rectTransform.DOSizeDelta(targetSize, 0.4f).SetEase(Ease.OutCubic).OnComplete(() => OnCompleteEaseOutWidth(quest));
    }
    private void OnCompleteEaseOutWidth(Quest quest) {
        RectTransform rectTransform = quest.questItem.transform as RectTransform;
        Vector2 targetSize = rectTransform.sizeDelta;
        targetSize.y = 0f;
        quest.questItem.contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        rectTransform.DOSizeDelta(targetSize, 0.3f).OnComplete(() => OnCompleteEaseOutHeight(quest));
    }
    private void OnCompleteEaseOutHeight(Quest quest) {
        if (quest.questItem != null) {
            ObjectPoolManager.Instance.DestroyObject(quest.questItem);
            quest.SetQuestItem(null);    
        }
    }
    #endregion

    #region Steps
    private void UpdateSteps(bool updateLayout = false) {
        UtilityScripts.Utilities.DestroyChildren(stepsParent);
        for (int i = 0; i < _quest.activeStepCollection.steps.Count; i++) {
            QuestStep step = _quest.activeStepCollection.steps[i];
            GameObject stepGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(questStepPrefab.name,
                Vector3.zero, Quaternion.identity, stepsParent);
            stepGO.transform.localScale = Vector3.one;
            QuestStepItem stepItem = stepGO.GetComponent<QuestStepItem>();
            stepItem.SetStep(step);
        }
        if (updateLayout) {
            UIManager.Instance.questUI.ReLayoutTutorials();    
        }
    }
    public void UpdateStepsDelayed(bool updateLayout = false) {
        StartCoroutine(UpdateStepsCoroutine(updateLayout));
    }
    private IEnumerator UpdateStepsCoroutine(bool updateLayout) {
        yield return new WaitForSecondsRealtime(1.5f);
        QuestStepItem[] children =
            GameUtilities.GetComponentsInDirectChildren<QuestStepItem>(stepsParent.gameObject);
        for (int i = 0; i < children.Length; i++) {
            QuestStepItem child = children[i];
            child.TweenOut();
            yield return new WaitForSecondsRealtime(0.2f);
        }
        yield return new WaitForSecondsRealtime(0.6f);
        UtilityScripts.Utilities.DestroyChildren(stepsParent);
        for (int i = 0; i < _quest.activeStepCollection.steps.Count; i++) {
            QuestStep step = _quest.activeStepCollection.steps[i];
            GameObject stepGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(questStepPrefab.name,
                Vector3.zero, Quaternion.identity, stepsParent);
            stepGO.transform.localScale = Vector3.one;
            QuestStepItem stepItem = stepGO.GetComponent<QuestStepItem>();
            stepItem.SetStep(step);
            stepItem.TweenIn();
            yield return new WaitForSecondsRealtime(0.2f);
        }
        if (updateLayout) {
            UIManager.Instance.questUI.ReLayoutTutorials();    
        }
    }
    #endregion

    #region Object Pool
    public override void Reset() {
        base.Reset();
        UtilityScripts.Utilities.DestroyChildren(stepsParent);
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        (transform as RectTransform).sizeDelta = _defaultSize;
    }
    #endregion
}
