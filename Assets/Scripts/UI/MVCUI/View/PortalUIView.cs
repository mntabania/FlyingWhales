using System;
using Ruinarch.MVCFramework;
using UnityEngine;
using UtilityScripts;

public class PortalUIView : MVCUIView {
    #region interface for listener
    public interface IListener {
        void OnClickReleaseAbility();
        void OnClickSummonDemon();
        void OnClickObtainBlueprint();
        void OnClickCancelReleaseAbility();
        void OnClickCancelSummonDemon();
        void OnClickCancelObtainBlueprint();
    }
    #endregion
    
    #region MVC Properties and functions to override
    /*
     * this will be the reference to the model 
     * */
    public PortalUIModel UIModel => _baseAssetModel as PortalUIModel;
    /*
     * Call this Create method to Initialize and instantiate the UI.
     * There's a callback on the controller if you want custom initialization
     * */
    public static void Create(Canvas p_canvas, PortalUIModel p_assets, Action<PortalUIView> p_onCreate) {
        var go = new GameObject(typeof(PortalUIView).ToString());
        var gui = go.AddComponent<PortalUIView>();
        var assetsInstance = Instantiate(p_assets);
        gui.Init(p_canvas, assetsInstance);
        if (p_onCreate != null)
        {
            p_onCreate.Invoke(gui);
        }
    }
    #endregion
    
    #region Subscribe/Unsubscribe for IListener
    public void Subscribe(IListener p_listener) {
        UIModel.onReleaseAbilityClicked += p_listener.OnClickReleaseAbility;
        UIModel.onSummonDemonClicked += p_listener.OnClickSummonDemon;
        UIModel.onObtainBlueprintClicked += p_listener.OnClickObtainBlueprint;
        UIModel.onCancelReleaseAbilityClicked += p_listener.OnClickCancelReleaseAbility;
        UIModel.onCancelSummonDemonClicked += p_listener.OnClickCancelSummonDemon;
        UIModel.onCancelObtainBlueprintClicked += p_listener.OnClickCancelObtainBlueprint;
    }
    public void Unsubscribe(IListener p_listener) {
        UIModel.onReleaseAbilityClicked -= p_listener.OnClickReleaseAbility;
        UIModel.onSummonDemonClicked -= p_listener.OnClickSummonDemon;
        UIModel.onObtainBlueprintClicked -= p_listener.OnClickObtainBlueprint;
        UIModel.onCancelReleaseAbilityClicked -= p_listener.OnClickCancelReleaseAbility;
        UIModel.onCancelSummonDemonClicked -= p_listener.OnClickCancelSummonDemon;
        UIModel.onCancelObtainBlueprintClicked -= p_listener.OnClickCancelObtainBlueprint;
    }
    #endregion

    public void ShowUnlockAbilityTimerAndHideButton(SkillData p_skillToUnlock) {
        UIModel.timerReleaseAbility.SetName($"{LocalizationManager.Instance.GetLocalizedValue("UI", "PortalUI", "release_ability_active")} {p_skillToUnlock.name}");
        UIModel.goTimerReleaseAbility.SetActive(true);
        UIModel.btnReleaseAbility.gameObject.SetActive(false);
    }
    public void ShowUnlockAbilityButtonAndHideTimer() {
        UIModel.goTimerReleaseAbility.SetActive(false);
        UIModel.btnReleaseAbility.gameObject.SetActive(true);
    }
}
