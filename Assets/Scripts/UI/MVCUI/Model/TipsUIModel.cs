using System;
using Ruinarch.MVCFramework;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;

public class TipsUIModel : MVCUIModel {

    public Action onCloseClicked;
    public Button btnClose;
    public Transform scrollViewContent;
    
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
