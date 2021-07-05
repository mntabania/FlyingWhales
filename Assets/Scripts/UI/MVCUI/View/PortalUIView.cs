using System;
using Inner_Maps.Location_Structures;
using Ruinarch.MVCFramework;
using UnityEngine;
using UtilityScripts;

public class PortalUIView : MVCUIView {
    #region interface for listener
    public interface IListener {
        void OnClickReleaseAbility();
        void OnClickUpgradePortal();
        void OnClickCancelReleaseAbility();
        void OnClickCancelUpgradePortal();
        void OnHoverOverCancelReleaseAbility();
        void OnHoverOutCancelReleaseAbility();
        void OnHoverOverCancelUpgradePortal();
        void OnHoverOutCancelUpgradePortal();
        void OnHoverOverUpgradePortal();
        void OnHoverOutUpgradePortal();
        void OnClickClose();
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
        UIModel.onUpgradePortalClicked += p_listener.OnClickUpgradePortal;
        UIModel.onCancelReleaseAbilityClicked += p_listener.OnClickCancelReleaseAbility;
        UIModel.onCancelUpgradePortalClicked += p_listener.OnClickCancelUpgradePortal;
        UIModel.onHoverOverCancelReleaseAbility += p_listener.OnHoverOverCancelReleaseAbility;
        UIModel.onHoverOutCancelReleaseAbility += p_listener.OnHoverOutCancelReleaseAbility;
        UIModel.onHoverOverCancelUpgradePortal += p_listener.OnHoverOverCancelUpgradePortal;
        UIModel.onHoverOutCancelUpgradePortal += p_listener.OnHoverOutCancelUpgradePortal;
        UIModel.onHoverOverUpgradePortal += p_listener.OnHoverOverUpgradePortal;
        UIModel.onHoverOutUpgradePortal += p_listener.OnHoverOutUpgradePortal;
        UIModel.onClickClose += p_listener.OnClickClose;
    }
    public void Unsubscribe(IListener p_listener) {
        UIModel.onReleaseAbilityClicked -= p_listener.OnClickReleaseAbility;
        UIModel.onUpgradePortalClicked -= p_listener.OnClickUpgradePortal;
        UIModel.onCancelReleaseAbilityClicked -= p_listener.OnClickCancelReleaseAbility;
        UIModel.onCancelUpgradePortalClicked -= p_listener.OnClickCancelUpgradePortal;
        UIModel.onHoverOverCancelReleaseAbility -= p_listener.OnHoverOverCancelReleaseAbility;
        UIModel.onHoverOutCancelReleaseAbility -= p_listener.OnHoverOutCancelReleaseAbility;
        UIModel.onHoverOverCancelUpgradePortal -= p_listener.OnHoverOverCancelUpgradePortal;
        UIModel.onHoverOutCancelUpgradePortal -= p_listener.OnHoverOutCancelUpgradePortal;
        UIModel.onHoverOverUpgradePortal -= p_listener.OnHoverOverUpgradePortal;
        UIModel.onHoverOutUpgradePortal -= p_listener.OnHoverOutUpgradePortal;
        UIModel.onClickClose -= p_listener.OnClickClose;
    }
    #endregion

    public void ShowUnlockAbilityTimerAndHideButton(SkillData p_skillToUnlock) {
        UIModel.timerReleaseAbility.RefreshName();
        UIModel.goTimerReleaseAbility.SetActive(true);
        UIModel.btnReleaseAbility.gameObject.SetActive(false);
    }
    public void ShowUnlockAbilityButtonAndHideTimer() {
        UIModel.goTimerReleaseAbility.SetActive(false);
        UIModel.btnReleaseAbility.gameObject.SetActive(true);
    }
    
    public void ShowUpgradePortalTimerAndHideButton() {
        UIModel.timerUpgradePortal.RefreshName();
        UIModel.goTimerUpgradePortal.SetActive(true);
        UIModel.btnUpgradePortal.gameObject.SetActive(false);
    }
    public void ShowUpgradePortalButtonAndHideTimer() {
        UIModel.goTimerUpgradePortal.SetActive(false);
        UIModel.btnUpgradePortal.gameObject.SetActive(true);
    }
    public void SetUpgradePortalBtnInteractable(bool p_state) {
        UIModel.btnUpgradePortal.interactable = p_state;
    }
}
