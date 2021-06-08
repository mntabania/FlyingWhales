using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class EventEquipButton : MonoBehaviour, IPointerClickHandler {

    private System.Action<Character, TileObject> _onLeftClick;
    private System.Action<Character, TileObject> _onRightClick;

    private Character m_owner;
    private TileObject m_targetEquip;

    public void SetData(Character p_owner, TileObject p_targetEquip) {
        m_owner = p_owner;
        m_targetEquip = p_targetEquip;
    }

    public void ClearData() {
        m_owner = null;
        m_targetEquip = null;
    }

    #region Pointer Actions
    public void AddPointerLeftClickAction(System.Action<Character, TileObject> p_action) {
        _onLeftClick += p_action;
    }
    public void AddPointerRightClickAction(System.Action<Character, TileObject> p_action) {
        _onRightClick += p_action;
    }
    public void OnPointerClick(PointerEventData eventData) {

        if (eventData.button == PointerEventData.InputButton.Left) {
            OnLeftClick();
        } else if (eventData.button == PointerEventData.InputButton.Right) {
            OnRightClick();
        }
    }
    public void OnClick(BaseEventData eventData) {
        OnPointerClick(eventData as PointerEventData);
    }
    public void OnLeftClick() {
        _onLeftClick?.Invoke(m_owner, m_targetEquip);
    }
    private void OnRightClick() {
        _onRightClick?.Invoke(m_owner, m_targetEquip);
    }
	#endregion

}
