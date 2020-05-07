using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using EZObjectPools;
using Quests;
using Tutorial;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class QuestUI : MonoBehaviour {
    
    [FormerlySerializedAs("tutorialQuestsParent")] [SerializeField] private Transform questsParent;
    [FormerlySerializedAs("tutorialQuestItemPrefab")] [SerializeField] private GameObject questItemPrefab;
    [SerializeField] private VerticalLayoutGroup _layoutGroup;
    
    
    public void Initialize() {
        UtilityScripts.Utilities.DestroyChildren(questsParent);
    }
    
    public QuestItem ShowQuest(Quest quest, bool insertAtTop = false) {
        GameObject questGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(questItemPrefab.name,
            Vector3.zero, Quaternion.identity, questsParent);
        if (insertAtTop) {
            questGO.transform.SetAsFirstSibling();
        }
        QuestItem questItem = questGO.GetComponent<QuestItem>();
        questItem.SetQuest(quest);
        questItem.TweenIn();
        return questItem;
    }
    public void HideQuestDelayed(Quest quest) {
        quest.questItem.TweenOutDelayed();
    }
    public void ReLayoutTutorials() {
        StartCoroutine(ReLayout());
    }
    
    private IEnumerator ReLayout() {
        _layoutGroup.enabled = false;
        yield return null;
        _layoutGroup.enabled = true;
    }

}
