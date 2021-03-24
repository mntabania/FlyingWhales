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
    public CanvasGroup canvasGroupUpgradeBtn;
    public GameObject goUpgradeBtnCover;

    public RectTransform rectFrame;
    public CanvasGroup canvasGroupFrameGlow;
    public CanvasGroup canvasGroupFrame;
    
    public Action onClickUpgrade;
    public Action onClickClose;
    public UIHoverPosition tooltipHoverPos;
    
    public Vector2 defaultFrameSize { get; private set; }
    
    void Awake() {
        defaultFrameSize = rectFrame.sizeDelta;
    }
    private void OnEnable() {
        btnUpgrade.onClick.AddListener(OnClickUpgrade);
        btnClose.onClick.AddListener(OnClickClose);
    }
    private void OnDisable() {
        btnUpgrade.onClick.RemoveListener(OnClickUpgrade);
        btnClose.onClick.RemoveListener(OnClickClose);
    }
    private void OnClickUpgrade() {
        onClickUpgrade?.Invoke();
    }
    private void OnClickClose() {
        onClickClose?.Invoke();
    }
}
