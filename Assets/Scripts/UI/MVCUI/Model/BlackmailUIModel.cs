using System;
using Ruinarch.MVCFramework;
using UnityEngine.UI;

public class BlackmailUIModel : MVCUIModel {

    public Action onCloseClicked;
    public Action onClickConfirm;
    
    public BlackmailUIItem[] blackmailUIItems;
    public Button btnClose;
    public Button btnConfirm;

    private void OnEnable() {
        btnClose.onClick.AddListener(OnClickClose);
        btnConfirm.onClick.AddListener(OnClickConfirm);
    }
    private void OnDisable() {
        btnClose.onClick.RemoveListener(OnClickClose);
        btnConfirm.onClick.RemoveListener(OnClickConfirm);
    }

    #region On Clicks
    private void OnClickClose() {
        onCloseClicked?.Invoke();
    }
    private void OnClickConfirm() {
        onClickConfirm?.Invoke();
    }
    #endregion
}
