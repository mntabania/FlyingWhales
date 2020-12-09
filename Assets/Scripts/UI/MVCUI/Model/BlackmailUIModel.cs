using System;
using Ruinarch.MVCFramework;
using UnityEngine.UI;

public class BlackmailUIModel : MVCUIModel {

    public Action onCloseClicked;
    
    public BlackmailUIItem[] blackmailUIItems;
    public Button btnClose;

    private void OnEnable() {
        btnClose.onClick.AddListener(OnClickClose);
    }
    private void OnDisable() {
        btnClose.onClick.RemoveListener(OnClickClose);
    }

    #region On Clicks
    private void OnClickClose() {
        onCloseClicked?.Invoke();
    }	
    #endregion
}
