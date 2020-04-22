using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using EZObjectPools;
using Tutorial;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour {
    
    [SerializeField] private Transform tutorialQuestsParent;
    [SerializeField] private GameObject tutorialQuestItemPrefab;
    [SerializeField] private VerticalLayoutGroup _layoutGroup;

    public void Initialize() {
        UtilityScripts.Utilities.DestroyChildren(tutorialQuestsParent);
    }
    
    public TutorialQuestItem ShowTutorialQuest(TutorialQuest tutorialQuest) {
        GameObject tutorialQuestGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(tutorialQuestItemPrefab.name,
            Vector3.zero, Quaternion.identity, tutorialQuestsParent);
        TutorialQuestItem tutorialQuestItem = tutorialQuestGO.GetComponent<TutorialQuestItem>();
        tutorialQuestItem.SetTutorialQuest(tutorialQuest);
        RectTransform rectTransform = tutorialQuestGO.transform as RectTransform;
        Vector2 targetSize = rectTransform.sizeDelta;
        rectTransform.sizeDelta = new Vector2(0f, rectTransform.sizeDelta.y);
        rectTransform.DOSizeDelta(targetSize, 0.4f).SetEase(Ease.InCubic);
        StartCoroutine(ReLayout());
        return tutorialQuestItem;
    }
    public void HideTutorialQuest(TutorialQuest tutorialQuest) {
        RectTransform rectTransform = tutorialQuest.tutorialQuestItem.transform as RectTransform;
        Vector2 targetSize = rectTransform.sizeDelta;
        targetSize.x = 0f;
        rectTransform.DOSizeDelta(targetSize, 0.4f).SetEase(Ease.OutCubic).OnComplete(() => OnCompleteEaseOutWidth(tutorialQuest));
    }
    private void OnCompleteEaseOutWidth(TutorialQuest tutorialQuest) {
        RectTransform rectTransform = tutorialQuest.tutorialQuestItem.transform as RectTransform;
        Vector2 targetSize = rectTransform.sizeDelta;
        targetSize.y = 0f;
        tutorialQuest.tutorialQuestItem.contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        rectTransform.DOSizeDelta(targetSize, 0.3f).OnComplete(() => OnCompleteEaseOutHeight(tutorialQuest));
    }
    private void OnCompleteEaseOutHeight(TutorialQuest tutorialQuest) {
        ObjectPoolManager.Instance.DestroyObject(tutorialQuest.tutorialQuestItem);
        tutorialQuest.SetTutorialQuestItem(null);
    }

    private IEnumerator ReLayout() {
        _layoutGroup.enabled = false;
        yield return null;
        _layoutGroup.enabled = true;
    }

}
