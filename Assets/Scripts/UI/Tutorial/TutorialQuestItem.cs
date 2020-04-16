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
    private void Awake() {
        _defaultSize = (transform as RectTransform).sizeDelta;
    }
    public void SetTutorialQuest(TutorialQuest tutorialQuest) {
        headerLbl.text = tutorialQuest.questName;
        for (int i = 0; i < tutorialQuest.steps.Count; i++) {
            TutorialQuestStep step = tutorialQuest.steps[i];
            GameObject stepGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(tutorialQuestStepPrefab.name,
                Vector3.zero, Quaternion.identity, stepsParent);
            TutorialQuestStepItem stepItem = stepGO.GetComponent<TutorialQuestStepItem>();
            stepItem.SetStep(step);
        }
    }
    public override void Reset() {
        base.Reset();
        UtilityScripts.Utilities.DestroyChildren(stepsParent);
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        (transform as RectTransform).sizeDelta = _defaultSize;
    }
}
