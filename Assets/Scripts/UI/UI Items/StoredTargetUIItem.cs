using System;
using Ruinarch.Custom_UI;
using TMPro;
using UnityEngine;

public class StoredTargetUIItem : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI lblName;
    [SerializeField] private RuinarchButton btnMain;
    [SerializeField] private RuinarchButton btnDelete;
    [SerializeField] private HoverHandler hoverHandler;

    private IStoredTarget _target;
    private Action<IStoredTarget> _onHoverOverItem;
    private Action<IStoredTarget> _onHoverOutItem;

    #region getters
    public IStoredTarget target => _target;
    #endregion
    
    private void Awake() {
        btnDelete.onClick.AddListener(OnClickDelete);
        btnMain.onClick.AddListener(OnClickItem);
    }
    public void Initialize(Action<IStoredTarget> p_onHoverOver, Action<IStoredTarget> p_onHoverOut) {
        _onHoverOverItem = p_onHoverOver;
        _onHoverOutItem = p_onHoverOut;
        hoverHandler.AddOnHoverOverAction(OnHoverOver);
        hoverHandler.AddOnHoverOutAction(OnHoverOut);
    }
    public void SetTarget(IStoredTarget p_target) {
        _target = p_target;
        lblName.text = $"{p_target.iconRichText} {p_target.name}";
    }
    public void UpdateName(IStoredTarget p_target) {
        lblName.text = $"{p_target.iconRichText} {p_target.name}";
    }

    private void OnClickItem() {
        UIManager.Instance.OpenObjectUI(_target);
    }
    private void OnClickDelete() {
        PlayerManager.Instance.player.storedTargetsComponent.Remove(_target);
    }
    private void OnHoverOver() {
        _onHoverOverItem?.Invoke(target);
    }
    private void OnHoverOut() {
        _onHoverOutItem?.Invoke(target);
    }
    
}
