using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using EZObjectPools;
using Inner_Maps;

public class LocationGridTileMouseEvents : PooledObject, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
    private LocationGridTile _owner;

    public void SetOwner(LocationGridTile p_owner) {
        _owner = p_owner;
    }
    public void OnPointerEnter(PointerEventData eventData) {
        _owner.mouseEventsComponent.OnHoverEnter();
    }
    public void OnPointerExit(PointerEventData eventData) {
        _owner.mouseEventsComponent.OnHoverExit();
    }
    public void OnPointerClick(PointerEventData eventData) {
        if(eventData.button == PointerEventData.InputButton.Right) {
            _owner.mouseEventsComponent.OnRightClick();
        }
    }
}
