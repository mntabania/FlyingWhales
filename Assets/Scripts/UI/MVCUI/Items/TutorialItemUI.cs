using System;
using EZObjectPools;
using TMPro;
using Tutorial;
using UnityEngine;
using UnityEngine.UI;

public class TutorialItemUI : PooledObject {

    public static System.Action<TutorialManager.Tutorial_Type> onClickTutorialItem;
    
    [SerializeField] private TextMeshProUGUI tutorialName;
    [SerializeField] private GameObject unreadTutorialGO;
    [SerializeField] private Button btnMain;

    private TutorialManager.Tutorial_Type _tutorialType;
    private void Awake() {
        btnMain.onClick.AddListener(OnClickItem);
    }
    public void Initialize(TutorialManager.Tutorial_Type p_type) {
        _tutorialType = p_type;
        UpdateReadObject();
        Messenger.AddListener<TutorialManager.Tutorial_Type>(TutorialSignals.TUTORIAL_READ, OnTutorialRead);
    }

    #region Listeners
    private void OnTutorialRead(TutorialManager.Tutorial_Type p_type) {
        if (_tutorialType == p_type) {
            UpdateReadObject();
        }
    }
    #endregion
    
    private void UpdateReadObject() {
        unreadTutorialGO.SetActive(SaveManager.Instance.savePlayerManager.currentSaveDataPlayer.HasTutorialBeenRead(_tutorialType));
    }
    private void OnClickItem() {
        onClickTutorialItem?.Invoke(_tutorialType);
    }
    public override void Reset() {
        base.Reset();
        Messenger.RemoveListener<TutorialManager.Tutorial_Type>(TutorialSignals.TUTORIAL_READ, OnTutorialRead);
    }
}