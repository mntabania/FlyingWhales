using Ruinarch.Custom_UI;
using Ruinarch.MVCFramework;

public class TemptUIModel : MVCUIModel {

    public System.Action<bool> onToggleDarkBlessing;
    public System.Action<bool> onToggleEmpower;
    public System.Action<bool> onToggleCleanseFlaws;
    public System.Action onClickClose;
    public System.Action onClickConfirm;
    
    public RuinarchToggle tglDarkBlessing;
    public RuinarchToggle tglEmpower;
    public RuinarchToggle tglCleanseFlaws;

    public RuinarchButton btnConfirm;
    public RuinarchButton btnClose;

    private void OnEnable() {
        btnClose.onClick.AddListener(OnClickClose);
        btnConfirm.onClick.AddListener(OnClickConfirm);
        tglDarkBlessing.onValueChanged.AddListener(OnToggleDarkBlessing);
        tglEmpower.onValueChanged.AddListener(OnToggleEmpower);
        tglCleanseFlaws.onValueChanged.AddListener(OnToggleCleanseFlaws);
    }
    private void OnDisable() {
        btnClose.onClick.RemoveListener(OnClickClose);
        btnConfirm.onClick.RemoveListener(OnClickConfirm);
        tglDarkBlessing.onValueChanged.RemoveListener(OnToggleDarkBlessing);
        tglEmpower.onValueChanged.RemoveListener(OnToggleEmpower);
        tglCleanseFlaws.onValueChanged.RemoveListener(OnToggleCleanseFlaws);
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
    #endregion
}
