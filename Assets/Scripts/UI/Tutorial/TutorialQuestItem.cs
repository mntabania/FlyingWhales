using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EZObjectPools;
using TMPro;
using Tutorial;
using UnityEngine;
using UnityEngine.UI;

public class TutorialQuestItem : PooledObject {

    [SerializeField] private TextMeshProUGUI headerLbl;
    [SerializeField] private Transform stepsParent;
    [SerializeField] private GameObject tutorialQuestStepPrefab;

    public ContentSizeFitter contentSizeFitter;
    
    private Vector2 _defaultSize;
    private TutorialQuest _tutorialQuest;
    private void Awake() {
        _defaultSize = (transform as RectTransform).sizeDelta;
    }
    public void SetTutorialQuest(TutorialQuest tutorialQuest) {
        _tutorialQuest = tutorialQuest;
        headerLbl.text = tutorialQuest.questName;
        UpdateSteps();
    }
    public void UpdateSteps(bool updateLayout = false) {
        UtilityScripts.Utilities.DestroyChildren(stepsParent);
        for (int i = 0; i < _tutorialQuest.activeStepCollection.steps.Count; i++) {
            TutorialQuestStep step = _tutorialQuest.activeStepCollection.steps[i];
            GameObject stepGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(tutorialQuestStepPrefab.name,
                Vector3.zero, Quaternion.identity, stepsParent);
            TutorialQuestStepItem stepItem = stepGO.GetComponent<TutorialQuestStepItem>();
            stepItem.SetStep(step);
        }
        if (updateLayout) {
            TutorialManager.Instance.tutorialUI.ReLayoutTutorials();    
        }
    }
    public void UpdateStepsDelayed(bool updateLayout = false) {
        StartCoroutine(UpdateStepsCoroutine(updateLayout));
    }
    private IEnumerator UpdateStepsCoroutine(bool updateLayout) {
        yield return new WaitForSecondsRealtime(1.5f);
        TutorialQuestStepItem[] children =
            UtilityScripts.GameUtilities.GetComponentsInDirectChildren<TutorialQuestStepItem>(stepsParent.gameObject);
        for (int i = 0; i < children.Length; i++) {
            TutorialQuestStepItem child = children[i];
            child.TweenOut();
            yield return new WaitForSecondsRealtime(0.2f);
        }
        yield return new WaitForSecondsRealtime(0.6f);
        UtilityScripts.Utilities.DestroyChildren(stepsParent);
        for (int i = 0; i < _tutorialQuest.activeStepCollection.steps.Count; i++) {
            TutorialQuestStep step = _tutorialQuest.activeStepCollection.steps[i];
            GameObject stepGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(tutorialQuestStepPrefab.name,
                Vector3.zero, Quaternion.identity, stepsParent);
            TutorialQuestStepItem stepItem = stepGO.GetComponent<TutorialQuestStepItem>();
            stepItem.SetStep(step);
            stepItem.TweenIn();
            yield return new WaitForSecondsRealtime(0.2f);
        }
        if (updateLayout) {
            TutorialManager.Instance.tutorialUI.ReLayoutTutorials();    
        }
    }
    
    public override void Reset() {
        base.Reset();
        UtilityScripts.Utilities.DestroyChildren(stepsParent);
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        (transform as RectTransform).sizeDelta = _defaultSize;
    }
}
