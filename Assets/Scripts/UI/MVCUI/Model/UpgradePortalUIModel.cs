using System;
using Ruinarch.Custom_UI;
using Ruinarch.MVCFramework;
using TMPro;
using UnityEngine;
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

    [Header("Timer")] 
    public GameObject goUpgradePortalTimer;
    public TimerItemUI timerUpgradePortal;
    public RuinarchButton btnCancelUpgradePortal;
    public HoverHandler hoverHandlerBtnCancelUpgradePortal;
    public System.Action onHoverOverCancelUpgradePortal;
    public System.Action onHoverOutCancelUpgradePortal;
    
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
    }
    private void OnDisable() {
        btnUpgrade.onClick.RemoveListener(OnClickUpgrade);
        btnClose.onClick.RemoveListener(OnClickClose);
        btnCancelUpgradePortal.onClick.RemoveListener(OnClickCancelUpgradePortal);
        hoverHandlerBtnCancelUpgradePortal.RemoveOnHoverOverAction(OnHoverOverCancelUpgradePortal);
        hoverHandlerBtnCancelUpgradePortal.RemoveOnHoverOutAction(OnHoverOutCancelUpgradePortal);
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
}
