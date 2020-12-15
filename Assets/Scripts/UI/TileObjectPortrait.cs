using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileObjectPortrait : MonoBehaviour, IPointerClickHandler {
    [SerializeField] private GameObject goHover;

    private TileObject _tileObject;
    private Action<TileObject> _onRightClickAction;
    private void OnEnable() {
        goHover.SetActive(false);
    }
    private void OnDisable() {
        _tileObject = null;
    }

    public void SetRightClickAction(Action<TileObject> p_rightClickAction) {
        _onRightClickAction = p_rightClickAction;
    }
    
    public void SetTileObject(TileObject p_tileObject) {
        _tileObject = p_tileObject;
    }
    
    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Right) {
            OnRightClick();
        }
    }

    private void OnRightClick() {
        if (_tileObject != null) {
            _onRightClickAction?.Invoke(_tileObject);
        }
    }
}
