using System;
using DG.Tweening;
using EZObjectPools;
using Ruinarch.Custom_UI;
using TMPro;
using Tutorial;
using UnityEngine;
using UnityEngine.UI;

public class TutorialItemUI : PooledObject {

    public static System.Action<TutorialManager.Tutorial_Type> onTutorialItemToggledOn;
    public static System.Action<TutorialManager.Tutorial_Type> onTutorialItemToggledOff;
    
    [SerializeField] private TextMeshProUGUI tutorialName;
    [SerializeField] private GameObject unreadTutorialGO;
    [SerializeField] private RuinarchToggle toggleMain;

    #region getters
    public RuinarchToggle toggle => toggleMain;
    public TutorialManager.Tutorial_Type tutorialType => _tutorialType;
    #endregion
    
    private TutorialManager.Tutorial_Type _tutorialType;
    private void Awake() {
        toggleMain.onValueChanged.AddListener(OnClickItem);
    }
    private void OnDisable() {
        unreadTutorialGO.transform.DOKill();
    }
    private void OnEnable() {
        // if (unreadTutorialGO.activeSelf) {
        //     PlayUnreadGameObjectAnimation();
        // }
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
            // if (unreadTutorialGO.activeSelf) {
            //     PlayUnreadGameObjectAnimation();
            // } else {
            //     StopUnreadGameObjectAnimation();
            // }
        }
    }
    private void OnClickItem(bool p_isOn) {
        if (p_isOn) {
            onTutorialItemToggledOn?.Invoke(_tutorialType);    
        } else {
            onTutorialItemToggledOff?.Invoke(_tutorialType);
        }
    }
    public void ManualSelect() {
        toggle.SetIsOnWithoutNotify(true);
        toggle.group.NotifyToggleOn(toggle, false);
        onTutorialItemToggledOn?.Invoke(_tutorialType);
    }
    // private void PlayUnreadGameObjectAnimation() {
    //     unreadTutorialGO.transform.DOScale(2f, 0.2f).SetLoops(-1, LoopType.Yoyo);
    // }
    // private void StopUnreadGameObjectAnimation() {
    //     unreadTutorialGO.transform.DOKill();
    // }
    public override void Reset() {
        base.Reset();
        Messenger.RemoveListener<TutorialManager.Tutorial_Type>(TutorialSignals.TUTORIAL_READ, OnTutorialRead);
    }
}