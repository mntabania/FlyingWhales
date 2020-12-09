using System;
using Ruinarch.MVCFramework;
using Ruinarch.Custom_UI;
using UnityEngine.UI;
using UnityEngine;

public class SchemeUIModel : MVCUIModel {

    public Action onCloseClicked;
    public Action onClickConfirm;
    public Action onClickBlackmail;
    public Action onClickTemptation;

    public GameObject schemeUIItemPrefab;
    public Button btnClose;
    public Button btnConfirm;
    public Button btnBlackmail;
    public Button btnTemptation;
    public RuinarchText txtTitle;
    public RuinarchText txtSuccessRate;
    public ScrollRect scrollViewSchemes;

    private void OnEnable() {
        btnClose.onClick.AddListener(OnClickClose);
        btnConfirm.onClick.AddListener(OnClickConfirm);
        btnBlackmail.onClick.AddListener(OnClickBlackmail);
        btnTemptation.onClick.AddListener(OnClickTemptation);
    }
    private void OnDisable() {
        btnClose.onClick.RemoveListener(OnClickClose);
        btnConfirm.onClick.RemoveListener(OnClickConfirm);
        btnBlackmail.onClick.RemoveListener(OnClickBlackmail);
        btnTemptation.onClick.RemoveListener(OnClickTemptation);
    }

    #region On Clicks
    private void OnClickClose() {
        onCloseClicked?.Invoke();
    }
    private void OnClickConfirm() {
        onClickConfirm?.Invoke();
    }
    private void OnClickBlackmail() {
        onClickBlackmail?.Invoke();
    }
    private void OnClickTemptation() {
        onClickTemptation?.Invoke();
    }
    #endregion
}
