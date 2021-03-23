using System;
using Ruinarch.Custom_UI;
using Ruinarch.MVCFramework;
using TMPro;
using UnityEngine.UI;

public class UpgradePortalUIModel : MVCUIModel {
    public TextMeshProUGUI lblTitle;
    public RuinarchButton btnUpgrade;
    public RuinarchButton btnClose;
    public ScrollRect scrollRectContent;
    public UpgradePortalItemUI[] items;
    
    public Action onClickUpgrade;
    public Action onClickClose;
    
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
