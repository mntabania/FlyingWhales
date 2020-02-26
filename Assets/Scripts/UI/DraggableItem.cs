using System.Collections;
using System.Collections.Generic;
using Ruinarch;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField] protected bool _isDraggable;
    [SerializeField] protected bool _isDragging;
    protected Vector2 _draggingObjectOriginalSize;

    protected RectTransform _draggingObject;

    protected object associatedObj;

    #region getters/setters
    public virtual bool isDraggable {
        get { return _isDraggable; }
    }
    #endregion

    private void Awake() {
        SetDraggable(true);
    }

    public virtual void SetAssociatedObject(object obj) {
        associatedObj = obj;
    }

    #region IBeginDragHandler Members
    public virtual void OnBeginDrag(PointerEventData eventData) {
        if (!isDraggable) {
            return;
        }
        InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Drag_Clicked);
    }
    #endregion

    #region IDragHandler Members
    public virtual void OnDrag(PointerEventData eventData) {
        if (!_isDragging) {
            return;
        }
        //Set dragging object on cursor
        var canvas = _draggingObject.GetComponentInParent<Canvas>();
        Vector3 worldPoint;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(canvas.GetComponent<RectTransform>(), eventData.position,
            canvas.worldCamera, out worldPoint);
        _draggingObject.position = worldPoint;
    }
    #endregion

    #region IEndDragHandler Members
    public virtual void OnEndDrag(PointerEventData eventData) {
        _isDragging = false;
        InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Drag_Hover);
        if (_draggingObject != null) {
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
                customDropzone.OnDrop(_draggingObject.gameObject);
                Destroy(_draggingObject.gameObject);
            } else {
                CancelDrag();
            }
        }
    }
    public virtual void CancelDrag() {
        _isDragging = false;
        Destroy(_draggingObject.gameObject);
    }
    #endregion

    public virtual void SetDraggable(bool state) {
        _isDraggable = state;
    }

    public virtual void OnPointerEnter(PointerEventData eventData) {
        if (isDraggable && !InputManager.Instance.isDraggingItem) {
            InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Drag_Hover);
        }
    }
    public virtual void OnPointerExit(PointerEventData eventData) {
        if (isDraggable && !InputManager.Instance.isDraggingItem) {
            InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
        }
    }
}

public class DragObject : MonoBehaviour {

    private IDragParentItem _parentItem;

    #region getters/setters
    public IDragParentItem parentItem {
        get { return _parentItem; }
        set { SetParentItem(value); }
    }
    #endregion

    private void SetParentItem(IDragParentItem parentItem) {
        _parentItem = parentItem;
        Messenger.Broadcast(Signals.DRAG_OBJECT_CREATED, this);
    }

    public void OnDestroy() {
        Messenger.Broadcast(Signals.DRAG_OBJECT_DESTROYED, this);
    }
}
