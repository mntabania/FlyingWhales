using System.Collections;
using System.Collections.Generic;
using Ruinarch;
using UnityEngine;
using UnityEngine.EventSystems;

public class RoleSlotItemDraggable : DraggableItem {

    [SerializeField] private RoleSlotItem roleSlotItem;

    //private bool isDraggableOverride = true; //used to set if the role slot shoul be draggable or not, outside normal conditions

    #region getters/setters
    public override bool isDraggable {
        get {
            return false; //Always disabled for now
            //if (!isDraggableOverride) {
            //    return false; //if the draggable override is set to false, do not allow drag
            //}
            //return !PlayerManager.Instance.player.roleSlots[roleSlotItem.slotJob].isSlotLocked; 
        }
    }
    #endregion

    private void Awake() {
        //roleSlotItem = gameObject.GetComponent<RoleSlotItem>();
        SetDraggable(isDraggable);
    }

    //public void SetDraggableOverride(bool state) {
    //    isDraggableOverride = state;
    //}

    #region Overrides
    public override void OnBeginDrag(PointerEventData eventData) {
        //base.OnBeginDrag(eventData);
        //_characterItem = null;
        if (!isDraggable) {
            return;
        }
        InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Drag_Clicked);
        //_characterItem = gameObject.GetComponent<PlayerCharacterItem>();
        CharacterPortrait portrait = roleSlotItem.portrait;
        GameObject clone = (GameObject)Instantiate(portrait.gameObject);
        _draggingObject = clone.GetComponent<RectTransform>();
        _draggingObject.gameObject.AddComponent<DragObject>().parentItem = roleSlotItem;

        //Put _dragging object into the dragging settlement
        _draggingObject.sizeDelta = portrait.gameObject.GetComponent<RectTransform>().rect.size;
        _draggingObject.SetParent(UIManager.Instance.gameObject.GetComponent<RectTransform>(), true);
        _isDragging = true;
    }
    public override void OnEndDrag(PointerEventData eventData) {
        if (!isDraggable) {
            return;
        }
        _isDragging = false;
        InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Drag_Hover);
        if (roleSlotItem != null && _draggingObject != null) {
            List<RaycastResult> newRaycastResults = new List<RaycastResult>();
            CustomDropZone customDropzone = null;
            EventSystem.current.RaycastAll(eventData, newRaycastResults);
            for (int i = 0; i < newRaycastResults.Count; i++) {
                customDropzone = newRaycastResults[i].gameObject.GetComponent<CustomDropZone>();
                if (customDropzone != null) {
                    break;
                }
            }

            if (customDropzone != null) {
                InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
                customDropzone.OnDrop(_draggingObject.gameObject);
                Destroy(_draggingObject.gameObject);
            } else {
                InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
                //PlayerManager.Instance.player.UnassignCharacterFromJob(roleSlotItem.slotJob);
                CancelDrag();
            }
        }
    }
    public override void CancelDrag() {
        base.CancelDrag();
        //_characterItem = null;
    }
    public override void SetDraggable(bool state) {
        if (isDraggable != state) {
            base.SetDraggable(state);
            //if (state) {
            //    _characterItem.portrait.SwitchBGToDraggable();
            //} else {
            //    _characterItem.portrait.SwitchBGToLocked();
            //}
        }
    }
    #endregion

    public override void OnPointerEnter(PointerEventData eventData) {
        if (roleSlotItem.associatedObj == null) {
            return;
        }
        base.OnPointerEnter(eventData);
    }
    public override void OnPointerExit(PointerEventData eventData) {
        if (roleSlotItem.associatedObj == null) {
            return;
        }
        base.OnPointerExit(eventData);
    }
}
