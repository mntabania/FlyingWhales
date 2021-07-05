using System;
using Ruinarch.Custom_UI;
using Ruinarch.MVCFramework;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UpgradePortalUIModel : MVCUIModel {
    public TextMeshProUGUI lblTitle;
    public RuinarchButton btnUpgrade;
    public RuinarchButton btnClose;
    public ScrollRect scrollRectContent;
    public HorizontalLayoutGroup contentLayoutGroup;
    public UpgradePortalItemUI[] items;
    public RectTransform rectWindow;
    public CanvasGroup canvasGroupWindow;
    public CanvasGroup canvasGroupUpgradeInteraction;
    public CanvasGroup canvasGroupCover;
    public GameObject goUpgradeBtnCover;

    public RectTransform rectFrame;
    public CanvasGroup canvasGroupFrameGlow;
    public CanvasGroup canvasGroupFrame;
    public UIHoverPosition tooltipHoverPos;
    [FormerlySerializedAs("lblChaoticEnergy")] public RuinarchText lblSpiritEnergy;

    [Header("Timer")] 
    public GameObject goUpgradePortalTimer;
    public TimerItemUI timerUpgradePortal;
    public RuinarchButton btnCancelUpgradePortal;
    public HoverHandler hoverHandlerBtnCancelUpgradePortal;
    public System.Action onHoverOverCancelUpgradePortal;
    public System.Action onHoverOutCancelUpgradePortal;

    [Header("Chaotic Energy")] 
    public GameObject goChaoticEnergyCapacity;
    public HoverHandler hoverHandlerChaoticEnergyCapacity;
    public RectTransform contentChaoticEnergyUpgrade;
    public CanvasGroup canvasGroupChaoticEnergyUpgrade;
    public TextMeshProUGUI lblChaoticEnergyCapacity;
    public Action onHoverOverChaoticEnergyCapacity;
    public Action onHoverOutChaoticEnergyCapacity;
    
    [Header("Awaken Ruinarch")] 
    public TextMeshProUGUI lblAwakenRuinarch;
    public CanvasGroup canvasGroupAwakenRuinarch;
    
    public Action onClickUpgrade;
    public Action onClickClose;
    public Action onClickCancelUpgradePortal;
    
    public Vector2 defaultFrameSize { get; private set; }
    
    void Awake() {
        defaultFrameSize = rectFrame.sizeDelta;
    }
    private void OnEnable() {
        btnUpgrade.onClick.AddListener(OnClickUpgrade);
        btnClose.onClick.AddListener(OnClickClose);
        btnCancelUpgradePortal.onClick.AddListener(OnClickCancelUpgradePortal);
        hoverHandlerBtnCancelUpgradePortal.AddOnHoverOverAction(OnHoverOverCancelUpgradePortal);
        hoverHandlerBtnCancelUpgradePortal.AddOnHoverOutAction(OnHoverOutCancelUpgradePortal);
        hoverHandlerChaoticEnergyCapacity.AddOnHoverOverAction(OnHoverOverUpgradeChaoticEnergyCapacity);
        hoverHandlerChaoticEnergyCapacity.AddOnHoverOutAction(OnHoverOutUpgradeChaoticEnergyCapacity);
    }
    private void OnDisable() {
        btnUpgrade.onClick.RemoveListener(OnClickUpgrade);
        btnClose.onClick.RemoveListener(OnClickClose);
        btnCancelUpgradePortal.onClick.RemoveListener(OnClickCancelUpgradePortal);
        hoverHandlerBtnCancelUpgradePortal.RemoveOnHoverOverAction(OnHoverOverCancelUpgradePortal);
        hoverHandlerBtnCancelUpgradePortal.RemoveOnHoverOutAction(OnHoverOutCancelUpgradePortal);
        hoverHandlerChaoticEnergyCapacity.RemoveOnHoverOverAction(OnHoverOverUpgradeChaoticEnergyCapacity);
        hoverHandlerChaoticEnergyCapacity.RemoveOnHoverOutAction(OnHoverOutUpgradeChaoticEnergyCapacity);
    }
    private void OnClickUpgrade() {
        onClickUpgrade?.Invoke();
    }
    private void OnClickClose() {
        onClickClose?.Invoke();
    }
    private void OnClickCancelUpgradePortal() {
        onClickCancelUpgradePortal?.Invoke();
    }
    private void OnHoverOverCancelUpgradePortal() {
        onHoverOverCancelUpgradePortal?.Invoke();
    }
    private void OnHoverOutCancelUpgradePortal() {
        onHoverOutCancelUpgradePortal?.Invoke();
    }
    private void OnHoverOverUpgradeChaoticEnergyCapacity() {
        onHoverOverChaoticEnergyCapacity?.Invoke();
    }
    private void OnHoverOutUpgradeChaoticEnergyCapacity() {
        onHoverOutChaoticEnergyCapacity?.Invoke();
    }
}
