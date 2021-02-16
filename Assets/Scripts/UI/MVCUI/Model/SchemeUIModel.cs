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
    public Action<UIHoverPosition> onHoverOverBlackmailBtn;
    public Action<UIHoverPosition> onHoverOverTemptBtn;
    public Action onHoverOverSuccessRate;
    public Action onHoverOutSuccessRate;
    public Action onHoverOutBlackmailBtn;
    public Action onHoverOutTemptBtn;

    public GameObject schemeUIItemPrefab;
    public Button btnClose;
    public Button btnConfirm;
    public Button btnBlackmail;
    public Button btnTemptation;
    public RuinarchText txtTitle;
    public RuinarchText txtSuccessRate;
    public ScrollRect scrollViewSchemes;
    public HoverHandler hoverHandlerBtnBlackmail;
    public HoverHandler hoverHandlerBtnTempt;
    public HoverHandler hoverHandlerSuccessRateText;

    public UIHoverPosition hoverPositionBlackmail;
    public UIHoverPosition hoverPositionTempt;
    
    private void OnEnable() {
        btnClose.onClick.AddListener(OnClickClose);
        btnConfirm.onClick.AddListener(OnClickConfirm);
        btnBlackmail.onClick.AddListener(OnClickBlackmail);
        btnTemptation.onClick.AddListener(OnClickTemptation);
        hoverHandlerBtnBlackmail.AddOnHoverOverAction(OnHoverOverBlackmailBtn);
        hoverHandlerBtnBlackmail.AddOnHoverOutAction(OnHoverOutBlackmailBtn);
        hoverHandlerBtnTempt.AddOnHoverOverAction(OnHoverOverTemptBtn);
        hoverHandlerBtnTempt.AddOnHoverOutAction(OnHoverOutTemptBtn);
        hoverHandlerSuccessRateText.AddOnHoverOverAction(OnHoverOverSuccessRate);
        hoverHandlerSuccessRateText.AddOnHoverOutAction(OnHoverOutSuccessRate);
    }
    private void OnDisable() {
        btnClose.onClick.RemoveListener(OnClickClose);
        btnConfirm.onClick.RemoveListener(OnClickConfirm);
        btnBlackmail.onClick.RemoveListener(OnClickBlackmail);
        btnTemptation.onClick.RemoveListener(OnClickTemptation);
        hoverHandlerBtnBlackmail.RemoveOnHoverOverAction(OnHoverOverBlackmailBtn);
        hoverHandlerBtnBlackmail.RemoveOnHoverOutAction(OnHoverOutBlackmailBtn);
        hoverHandlerBtnTempt.RemoveOnHoverOverAction(OnHoverOverTemptBtn);
        hoverHandlerBtnTempt.RemoveOnHoverOutAction(OnHoverOutTemptBtn);
        hoverHandlerSuccessRateText.RemoveOnHoverOverAction(OnHoverOverSuccessRate);
        hoverHandlerSuccessRateText.RemoveOnHoverOutAction(OnHoverOutSuccessRate);
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
    private void OnHoverOverBlackmailBtn() {
        onHoverOverBlackmailBtn?.Invoke(hoverPositionBlackmail);
    }
    private void OnHoverOutBlackmailBtn() {
        onHoverOutBlackmailBtn?.Invoke();
    }
    private void OnHoverOverTemptBtn() {
        onHoverOverTemptBtn?.Invoke(hoverPositionTempt);
    }
    private void OnHoverOutTemptBtn() {
        onHoverOutTemptBtn?.Invoke();
    }
    private void OnHoverOverSuccessRate() {
        onHoverOverSuccessRate?.Invoke();
    }
    private void OnHoverOutSuccessRate() {
        onHoverOutSuccessRate?.Invoke();
    }
    #endregion
}
