using Ruinarch.Custom_UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using EZObjectPools;

public class SchemeUIItem : PooledObject {

    private System.Action<SchemeUIItem> onClickMinusAction;
    private System.Action<SchemeUIItem> onHoverEnterAction;
    private System.Action<SchemeUIItem> onHoverExitAction;

    public TextMeshProUGUI txtName;
    public TextMeshProUGUI txtSucessRate;
    public RuinarchButton btnMinus;

    private float _successRate;
    private float _baseSuccessRate;

    #region getters
    public float successRate => _successRate;
    public float baseSuccessRate => _baseSuccessRate;
    #endregion

    private void OnEnable() {
        btnMinus.onClick.AddListener(OnClickMinus);
    }
    private void OnDisable() {
        btnMinus.onClick.RemoveListener(OnClickMinus);
    }

    public void SetItemDetails(string p_text, float p_successRate, float p_baseSuccessRate) {
        _successRate = p_successRate;
        _baseSuccessRate = p_baseSuccessRate;
        txtName.text = p_text;
        txtSucessRate.text = $"<color=green>+{p_successRate.ToString("N1")}%</color>";
    }
    public void SetClickMinusAction(System.Action<SchemeUIItem> action) {
        onClickMinusAction = action;
    }
    public void SetOnHoverEnterAction(System.Action<SchemeUIItem> action) {
        onHoverEnterAction = action;
    }
    public void SetOnHoverExitAction(System.Action<SchemeUIItem> action) {
        onHoverExitAction = action;
    }

    #region Button Clicks
    private void OnClickMinus() {
        onClickMinusAction?.Invoke(this);
    }
    #endregion

    #region Hover
    public void OnHoverEnter() {
        onHoverEnterAction?.Invoke(this);
    }
    public void OnHoverExit() {
        onHoverExitAction?.Invoke(this);
    }
    #endregion

    #region Object Pool
    public override void Reset() {
        base.Reset();
        onClickMinusAction = null;
        onHoverEnterAction = null;
        onHoverExitAction = null;
        txtName.text = string.Empty;
        txtSucessRate.text = string.Empty;
    }
    #endregion
}
