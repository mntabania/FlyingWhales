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

    #region getters
    public float successRate => _successRate;
    #endregion

    private void OnEnable() {
        btnMinus.onClick.AddListener(OnClickMinus);
    }
    private void OnDisable() {
        btnMinus.onClick.RemoveListener(OnClickMinus);
    }

    public void SetItemDetails(string text, float successRate) {
        _successRate = successRate;
        txtName.text = text;
        txtSucessRate.text = $"+{successRate.ToString("N1")}%";
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
