using Ruinarch.Custom_UI;
using Ruinarch.MVCFramework;
using UnityEngine;

public class TemptUIModel : MVCUIModel {

    public System.Action<bool> onToggleDarkBlessing;
    public System.Action<bool> onToggleEmpower;
    public System.Action<bool> onToggleCleanseFlaws;
    public System.Action onHoverDarkBlessing;
    public System.Action onHoverEmpower;
    public System.Action onHoverCleanseFlaws;
    public System.Action onHoverOutTemptation;

    public System.Action onClickClose;
    public System.Action onClickConfirm;
    
    public RuinarchToggle tglDarkBlessing;
    public RuinarchToggle tglEmpower;
    public RuinarchToggle tglCleanseFlaws;

    public HoverHandler hvrDarkBlessing;
    public HoverHandler hvrEmpower;
    public HoverHandler hvrCleanseFlaws;

    public GameObject coverDarkBlessing;
    public GameObject coverEmpower;
    public GameObject coverCleanseFlaws;

    public RuinarchButton btnConfirm;
    public RuinarchButton btnClose;

    private void OnEnable() {
        btnClose.onClick.AddListener(OnClickClose);
        btnConfirm.onClick.AddListener(OnClickConfirm);
        tglDarkBlessing.onValueChanged.AddListener(OnToggleDarkBlessing);
        tglEmpower.onValueChanged.AddListener(OnToggleEmpower);
        tglCleanseFlaws.onValueChanged.AddListener(OnToggleCleanseFlaws);
        hvrDarkBlessing.AddOnHoverOverAction(OnHoverEnterDarkBlessing);
        hvrEmpower.AddOnHoverOverAction(OnHoverEnterEmpower);
        hvrCleanseFlaws.AddOnHoverOverAction(OnHoverEnterCleanseFlaws);
        hvrDarkBlessing.AddOnHoverOutAction(OnHoverOutTemptation);
        hvrEmpower.AddOnHoverOutAction(OnHoverOutTemptation);
        hvrCleanseFlaws.AddOnHoverOutAction(OnHoverOutTemptation);
    }
    private void OnDisable() {
        btnClose.onClick.RemoveListener(OnClickClose);
        btnConfirm.onClick.RemoveListener(OnClickConfirm);
        tglDarkBlessing.onValueChanged.RemoveListener(OnToggleDarkBlessing);
        tglEmpower.onValueChanged.RemoveListener(OnToggleEmpower);
        tglCleanseFlaws.onValueChanged.RemoveListener(OnToggleCleanseFlaws);
        hvrDarkBlessing.RemoveOnHoverOverAction(OnHoverEnterDarkBlessing);
        hvrEmpower.RemoveOnHoverOverAction(OnHoverEnterEmpower);
        hvrCleanseFlaws.RemoveOnHoverOverAction(OnHoverEnterCleanseFlaws);
        hvrDarkBlessing.RemoveOnHoverOutAction(OnHoverOutTemptation);
        hvrEmpower.RemoveOnHoverOutAction(OnHoverOutTemptation);
        hvrCleanseFlaws.RemoveOnHoverOutAction(OnHoverOutTemptation);
    }

    #region On Clicks
    private void OnClickClose() {
        onClickClose?.Invoke();
    }
    private void OnClickConfirm() {
        onClickConfirm?.Invoke();
    }
    private void OnToggleDarkBlessing(bool p_isOn) {
        onToggleDarkBlessing?.Invoke(p_isOn);
    }
    private void OnToggleEmpower(bool p_isOn) {
        onToggleEmpower?.Invoke(p_isOn);
    }
    private void OnToggleCleanseFlaws(bool p_isOn) {
        onToggleCleanseFlaws?.Invoke(p_isOn);
    }
    private void OnHoverEnterDarkBlessing() {
        onHoverDarkBlessing?.Invoke();
    }
    private void OnHoverEnterEmpower() {
        onHoverEmpower?.Invoke();
    }
    private void OnHoverEnterCleanseFlaws() {
        onHoverCleanseFlaws?.Invoke();
    }
    private void OnHoverOutTemptation() {
        onHoverOutTemptation?.Invoke();
    }
    #endregion
}
