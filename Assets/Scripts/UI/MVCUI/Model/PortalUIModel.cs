using System;
using Ruinarch.Custom_UI;
using Ruinarch.MVCFramework;
using UnityEngine;

public class PortalUIModel : MVCUIModel {

    public RuinarchButton btnReleaseAbility;
    public RuinarchButton btnUpgradePortal;

    public TimerItemUI timerReleaseAbility;
    public TimerItemUI timerUpgradePortal;
    
    public GameObject goTimerReleaseAbility;
    public GameObject goTimerUpgradePortal;
    
    public RuinarchButton btnCancelReleaseAbility;
    public RuinarchButton btnCancelUpgradePortal;
    public RuinarchButton btnClose;

    public HoverHandler hoverHandlerBtnCancelReleaseAbility;
    public HoverHandler hoverHandlerBtnCancelUpgradePortal;
    
    public HoverHandler hoverHandlerBtnUpgradePortal;

    public System.Action onReleaseAbilityClicked;
    public System.Action onUpgradePortalClicked;
    public System.Action onCancelReleaseAbilityClicked;
    public System.Action onCancelUpgradePortalClicked;
    public System.Action onHoverOverCancelReleaseAbility;
    public System.Action onHoverOutCancelReleaseAbility;
    public System.Action onHoverOverCancelUpgradePortal;
    public System.Action onHoverOutCancelUpgradePortal;
    public System.Action onHoverOverUpgradePortal;
    public System.Action onHoverOutUpgradePortal;
    
    public System.Action onClickClose;


    void Awake() {
        btnReleaseAbility.SetButtonLabelName(LocalizationManager.Instance.GetLocalizedValue("UI", "PortalUI", "release_ability"));
        btnUpgradePortal.SetButtonLabelName(LocalizationManager.Instance.GetLocalizedValue("UI", "PortalUI", "upgrade_portal"));
    }
    
    private void OnEnable() {
        btnReleaseAbility.onClick.AddListener(ClickReleaseAbility);
        btnUpgradePortal.onClick.AddListener(ClickUpgradePortal);
        btnCancelReleaseAbility.onClick.AddListener(ClickCancelReleaseAbility);
        btnCancelUpgradePortal.onClick.AddListener(ClickCancelUpgradePortal);
        hoverHandlerBtnCancelReleaseAbility.AddOnHoverOverAction(OnHoverOverCancelReleaseAbility);
        hoverHandlerBtnCancelReleaseAbility.AddOnHoverOutAction(OnHoverOutCancelReleaseAbility);
        hoverHandlerBtnCancelUpgradePortal.AddOnHoverOverAction(OnHoverOverCancelUpgradePortal);
        hoverHandlerBtnCancelUpgradePortal.AddOnHoverOutAction(OnHoverOutCancelUpgradePortal);
        
        hoverHandlerBtnUpgradePortal.AddOnHoverOverAction(OnHoverOverUpgradePortal);
        hoverHandlerBtnUpgradePortal.AddOnHoverOutAction(OnHoverOutUpgradePortal);
        
        btnClose.onClick.AddListener(OnClickClose);
    }
    private void OnDisable() {
        btnReleaseAbility.onClick.RemoveListener(ClickReleaseAbility);
        btnUpgradePortal.onClick.RemoveListener(ClickUpgradePortal);
        btnCancelReleaseAbility.onClick.RemoveListener(ClickCancelReleaseAbility);
        btnCancelUpgradePortal.onClick.RemoveListener(ClickCancelUpgradePortal);
        hoverHandlerBtnCancelReleaseAbility.RemoveOnHoverOverAction(OnHoverOverCancelReleaseAbility);
        hoverHandlerBtnCancelReleaseAbility.RemoveOnHoverOutAction(OnHoverOutCancelReleaseAbility);
        hoverHandlerBtnCancelUpgradePortal.RemoveOnHoverOverAction(OnHoverOverCancelUpgradePortal);
        hoverHandlerBtnCancelUpgradePortal.RemoveOnHoverOutAction(OnHoverOutCancelUpgradePortal);
        hoverHandlerBtnUpgradePortal.RemoveOnHoverOverAction(OnHoverOverUpgradePortal);
        hoverHandlerBtnUpgradePortal.RemoveOnHoverOutAction(OnHoverOutUpgradePortal);
        btnClose.onClick.RemoveListener(OnClickClose);
    }
    private void ClickReleaseAbility() {
        onReleaseAbilityClicked?.Invoke();
    }
    private void ClickUpgradePortal() {
        onUpgradePortalClicked?.Invoke();
    }

    private void ClickCancelReleaseAbility() {
        onCancelReleaseAbilityClicked?.Invoke();
    }
    private void ClickCancelUpgradePortal() {
        onCancelUpgradePortalClicked?.Invoke();
    }
    private void OnHoverOverCancelReleaseAbility() {
        onHoverOverCancelReleaseAbility?.Invoke();
    }
    private void OnHoverOutCancelReleaseAbility() {
        onHoverOutCancelReleaseAbility?.Invoke();
    }
    private void OnHoverOverCancelUpgradePortal() {
        onHoverOverCancelUpgradePortal?.Invoke();
    }
    private void OnHoverOutCancelUpgradePortal() {
        onHoverOutCancelUpgradePortal?.Invoke();
    }
    
    private void OnHoverOverUpgradePortal() {
        onHoverOverUpgradePortal?.Invoke();
    }
    private void OnHoverOutUpgradePortal() {
        onHoverOutUpgradePortal?.Invoke();
    }
    
    private void OnClickClose() {
        onClickClose?.Invoke();
    }
}
