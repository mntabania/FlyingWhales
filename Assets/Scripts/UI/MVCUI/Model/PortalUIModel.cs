using System;
using Ruinarch.Custom_UI;
using Ruinarch.MVCFramework;
using UnityEngine;

public class PortalUIModel : MVCUIModel {

    public RuinarchButton btnReleaseAbility;
    public RuinarchButton btnSummonDemon;
    public RuinarchButton btnObtainBlueprint;

    public TimerItemUI timerReleaseAbility;
    public TimerItemUI timerSummonDemon;
    public TimerItemUI timerObtainBlueprint;
    
    public GameObject goTimerReleaseAbility;
    public GameObject goTimerSummonDemon;
    public GameObject goTimerObtainBlueprint;
    
    public RuinarchButton btnCancelReleaseAbility;
    public RuinarchButton btnCancelSummonDemon;
    public RuinarchButton btnCancelObtainBlueprint;

    public System.Action onReleaseAbilityClicked;
    public System.Action onSummonDemonClicked;
    public System.Action onObtainBlueprintClicked;
    public System.Action onCancelReleaseAbilityClicked;
    public System.Action onCancelSummonDemonClicked;
    public System.Action onCancelObtainBlueprintClicked;


    void Awake() {
        btnReleaseAbility.SetButtonLabelName(LocalizationManager.Instance.GetLocalizedValue("UI", "PortalUI", "release_ability"));
        btnSummonDemon.SetButtonLabelName(LocalizationManager.Instance.GetLocalizedValue("UI", "PortalUI", "summon_demon"));
        btnObtainBlueprint.SetButtonLabelName(LocalizationManager.Instance.GetLocalizedValue("UI", "PortalUI", "obtain_blueprint"));
    }
    
    private void OnEnable() {
        btnReleaseAbility.onClick.AddListener(ClickReleaseAbility);
        btnSummonDemon.onClick.AddListener(ClickSummonDemon);
        btnObtainBlueprint.onClick.AddListener(ClickObtainBlueprint);
        btnCancelReleaseAbility.onClick.AddListener(ClickCancelReleaseAbility);
        btnCancelSummonDemon.onClick.AddListener(ClickCancelSummonDemon);
        btnCancelObtainBlueprint.onClick.AddListener(ClickCancelObtainBlueprint);
    }
    private void OnDisable() {
        btnReleaseAbility.onClick.RemoveListener(ClickReleaseAbility);
        btnSummonDemon.onClick.RemoveListener(ClickSummonDemon);
        btnObtainBlueprint.onClick.RemoveListener(ClickObtainBlueprint);
        btnCancelReleaseAbility.onClick.RemoveListener(ClickCancelReleaseAbility);
        btnCancelSummonDemon.onClick.RemoveListener(ClickCancelSummonDemon);
        btnCancelObtainBlueprint.onClick.RemoveListener(ClickCancelObtainBlueprint);
    }
    private void ClickReleaseAbility() {
        onReleaseAbilityClicked?.Invoke();
    }
    private void ClickSummonDemon() {
        onSummonDemonClicked?.Invoke();
    }
    private void ClickObtainBlueprint() {
        onObtainBlueprintClicked?.Invoke();
    }
    
    private void ClickCancelReleaseAbility() {
        onCancelReleaseAbilityClicked?.Invoke();
    }
    private void ClickCancelSummonDemon() {
        onCancelSummonDemonClicked?.Invoke();
    }
    private void ClickCancelObtainBlueprint() {
        onCancelObtainBlueprintClicked?.Invoke();
    }
}
