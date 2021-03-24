using System;
using EZObjectPools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BaseLocationPortrait : PooledObject, IPointerClickHandler {
    [SerializeField] private Image portrait;
    [SerializeField] private GameObject hoverObj;
    [SerializeField] private bool disableInteraction;
    [SerializeField] private HoverHandler hoverHandler;
    
    private System.Action _leftClickAction;
    private System.Action _rightClickAction;
    private System.Action _onHoverOverAction;
    private System.Action _onHoverOutAction;
    
    private void Awake() {
        hoverHandler.AddOnHoverOverAction(OnHoverOver);
        hoverHandler.AddOnHoverOutAction(OnHoverOut);
    }
    public void SetPortrait(STRUCTURE_TYPE landmarkType) {
        portrait.sprite = LandmarkManager.Instance.GetStructureData(landmarkType).structureSprite;
    }

    #region Interaction
    public void AddLeftClickAction(System.Action p_action) {
        _leftClickAction += p_action;
    }
    public void AddRightClickAction(System.Action p_action) {
        _rightClickAction += p_action;
    }
    public void OnPointerClick(PointerEventData eventData) {
        if (!disableInteraction) {
            if (eventData.button == PointerEventData.InputButton.Left) {
                _leftClickAction?.Invoke();
            } else if (eventData.button == PointerEventData.InputButton.Right) {
                _rightClickAction?.Invoke();
            }
        }
    }
    private void OnHoverOver() {
        if (!hoverObj.activeSelf) {
            hoverObj.gameObject.SetActive(true);    
        }
        _onHoverOverAction?.Invoke();
    }
    private void OnHoverOut() {
        if (hoverObj.activeSelf) {
            hoverObj.gameObject.SetActive(false);    
        }
        _onHoverOutAction?.Invoke();
    }
    public void AddHoverOverAction(System.Action p_action) {
        _onHoverOverAction += p_action;
    }
    public void AddHoverOutAction(System.Action p_action) {
        _onHoverOutAction += p_action;
    }
    public void RemoveHoverOverAction(System.Action p_action) {
        _onHoverOverAction -= p_action;
    }
    public void RemoveHoverOutAction(System.Action p_action) {
        _onHoverOutAction -= p_action;
    }
    #endregion

    #region Object Pools
    public override void Reset() {
        base.Reset();
        _leftClickAction = null;
        _rightClickAction = null;
    }
    #endregion
}
