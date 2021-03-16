using System;
using Ruinarch.Custom_UI;
using Ruinarch.MVCFramework;

public class UnlockMinionUIModel : MVCUIModel {

    public System.Action onClickClose;
    
    public UnlockMinionItemUI[] minionItems;
    public RuinarchButton btnClose;
    private void OnEnable() {
        btnClose.onClick.AddListener(OnClickClose);
    }
    private void OnDisable() {
        btnClose.onClick.RemoveListener(OnClickClose);
    }

    private void OnClickClose() {
        onClickClose?.Invoke();
    }
}
