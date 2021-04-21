using System;
using EZObjectPools;
using Ruinarch.Custom_UI;
using TMPro;
using Tutorial;
using UnityEngine;
using UnityEngine.UI;

public class TutorialItemUI : PooledObject {

    public static System.Action<TutorialManager.Tutorial_Type> onClickTutorialItem;
    
    [SerializeField] private TextMeshProUGUI tutorialName;
    [SerializeField] private GameObject unreadTutorialGO;
    [SerializeField] private RuinarchToggle toggleMain;

    private TutorialManager.Tutorial_Type _tutorialType;
    private void Awake() {
        toggleMain.onValueChanged.AddListener(OnClickItem);
    }
    public void Initialize(TutorialManager.Tutorial_Type p_type, ToggleGroup p_group) {
        _tutorialType = p_type;
        tutorialName.text = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(p_type.ToString());
        toggleMain.group = p_group;
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
        if (SaveManager.Instance != null) {
            unreadTutorialGO.SetActive(!SaveManager.Instance.savePlayerManager.currentSaveDataPlayer.HasTutorialBeenRead(_tutorialType));    
        }
    }
    private void OnClickItem(bool p_isOn) {
        if (p_isOn) {
            onClickTutorialItem?.Invoke(_tutorialType);    
        }
    }
    public override void Reset() {
        base.Reset();
        Messenger.RemoveListener<TutorialManager.Tutorial_Type>(TutorialSignals.TUTORIAL_READ, OnTutorialRead);
    }
}