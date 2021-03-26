using System;
using Ruinarch.Custom_UI;
using Ruinarch.MVCFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePortalUIModel : MVCUIModel {
    public TextMeshProUGUI lblTitle;
    public TextMeshProUGUI lblCost;
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
    }
    private void OnDisable() {
        btnUpgrade.onClick.RemoveListener(OnClickUpgrade);
        btnClose.onClick.RemoveListener(OnClickClose);
        btnCancelUpgradePortal.onClick.RemoveListener(OnClickCancelUpgradePortal);
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
}
