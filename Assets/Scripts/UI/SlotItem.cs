using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotItem : MonoBehaviour {

    public object placedObject { get; private set; }
    public System.Type neededType { get; private set; }

    public delegate bool OtherValidation(object obj, SlotItem slotItem); //this is for when special conditions are needed to determine whether an object is valid for this slot (i.e Character must have traits, or be a specific race)
    public OtherValidation isObjectValidForSlot;

    //public SlotItemDropEvent onItemDroppedValid;
    private ItemDroppedCallback itemDroppedCallback = new ItemDroppedCallback();
    private ItemDroppedOutCallback itemDroppedOutCallback = new ItemDroppedOutCallback();

    [Space(10)]
    [Header("Slot Elements")]
    //[SerializeField] private string neededTypeStr;
    public CharacterPortrait portrait;
    public AreaEmblem areaEmblem;
    public FactionEmblem factionEmblem;
    public Image image;
    public CustomDropZone dropZone;
    public SlotItemDraggable draggable;

    [Space(10)]
    [Header("Misc")]
    [SerializeField] private GameObject glowGO;
    [SerializeField] private GameObject validPortraitGO;
    [SerializeField] private GameObject invalidPortraitGO;

    public int slotIndex { get; private set; }
    private string hoverInfo;

    #region Monobehaviours
    private void OnEnable() {
        AddListeners();
    }
    private void OnDisable() {
        RemoveListeners();
    }
    #endregion

    #region Drop Zone
    public void OnDropItemAtDropZone(GameObject go) { //this is used to filter if the dragged object is valid for this slot
        DragObject dragObj = go.GetComponent<DragObject>();
        if (dragObj == null) {
            return;
        }
        IDragParentItem parentItem = dragObj.parentItem;
        if (parentItem != null) {
            if (IsObjectValidForSlot(parentItem.associatedObj)) {
                OnDroppedItemValid(parentItem);
            }
            //else {
            //    Messenger.Broadcast<string, bool>(Signals.SHOW_POPUP_MESSAGE, "This slot requires a " + GetTypeString(neededType), true);
            //}
        }
    }
    //private void SuccessfulDropZoneDrop(IDragParentItem parentItem) {
    //    if (onItemDroppedValid != null) {
    //        onItemDroppedValid.Invoke(parentItem);
    //    }
    //}
    #endregion

    #region Core Functions
    public void SetSlotIndex(int index) {
        slotIndex = index;
    }
    public void SetNeededType(System.Type neededType) {
        this.neededType = neededType;
        //neededTypeStr = neededType.ToString();
    }
    public void OnDroppedItemValid(IDragParentItem item) {
        PlaceObject(item.associatedObj);
        if (itemDroppedCallback != null) {
            itemDroppedCallback.Invoke(item.associatedObj, slotIndex);
        }
    }
    public void PlaceObject(object associatedObj) {
        placedObject = associatedObj;
        if (placedObject == null) {
            ClearSlot(true);
            return;
        }
        if (associatedObj is Character) {
            factionEmblem.gameObject.SetActive(false);
            areaEmblem.gameObject.SetActive(false);
            portrait.gameObject.SetActive(true);
            image.gameObject.SetActive(false);
            portrait.GeneratePortrait((associatedObj as Character));
            hoverInfo = (associatedObj as Character).name;
        } 
        // else if (associatedObj is SpecialToken) {
        //     factionEmblem.gameObject.SetActive(false);
        //     areaEmblem.gameObject.SetActive(false);
        //     portrait.gameObject.SetActive(false);
        //     hoverInfo = (associatedObj as SpecialToken).name;
        //     image.gameObject.SetActive(true);
        // }
    }
    public void ClearSlot(bool keepType = false) {
        if (!keepType) {
            neededType = null;
        }
        placedObject = null;
        factionEmblem.gameObject.SetActive(false);
        areaEmblem.gameObject.SetActive(false);
        portrait.gameObject.SetActive(false);
    }
    private bool IsObjectValidForSlot(object obj) {
        if (neededType != null && neededType.IsInstanceOfType(obj)) {
            if (isObjectValidForSlot == null || isObjectValidForSlot(obj, this)) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Draggable Item
    public void OnItemDroppedOut() {
        if (itemDroppedOutCallback != null) {
            itemDroppedOutCallback.Invoke(placedObject, slotIndex);
        }
    }
    #endregion

    #region Utilities
    private string GetTypeString(System.Type type) {
        if (type == null) {
            return "null";
        }
        if (type == typeof(Minion)) {
            return "Minion";
        } else if (type == typeof(Character)) {
            return "Character";
        } else {
            return type.ToString();
        }
    }
    public void HideVisuals() {
        portrait.gameObject.SetActive(false);
        factionEmblem.gameObject.SetActive(false);
        areaEmblem.gameObject.SetActive(false);
    }
    public void ShowObjectInfo() {
        if (placedObject != null) {
            UIManager.Instance.ShowSmallInfo(hoverInfo);
        }
    }
    public void HideObjectInfo() {
        if (placedObject != null) {
            UIManager.Instance.HideSmallInfo();
        }
    }
    private void AddListeners() {
        Messenger.AddListener<DragObject>(Signals.DRAG_OBJECT_CREATED, OnDragObjectCreated);
        Messenger.AddListener<DragObject>(Signals.DRAG_OBJECT_DESTROYED, OnDragObjectDestroyed);
    }
    private void RemoveListeners() {
        Messenger.RemoveListener<DragObject>(Signals.DRAG_OBJECT_CREATED, OnDragObjectCreated);
        Messenger.RemoveListener<DragObject>(Signals.DRAG_OBJECT_DESTROYED, OnDragObjectDestroyed);
    }
    private void OnDragObjectCreated(DragObject obj) {
        if (dropZone != null && dropZone.isEnabled && neededType != null && IsObjectValidForSlot(obj.parentItem.associatedObj)) {
            //glowGO.SetActive(true);
            validPortraitGO.SetActive(true);
            invalidPortraitGO.SetActive(false);
        } else {
            validPortraitGO.SetActive(false);
            invalidPortraitGO.SetActive(true);
        }
    }
    private void OnDragObjectDestroyed(DragObject obj) {
        //glowGO.SetActive(false);
        validPortraitGO.SetActive(false);
        invalidPortraitGO.SetActive(false);
    }
    public void SetOtherValidation(OtherValidation validation) {
        isObjectValidForSlot = validation;
    }
    public void SetItemDroppedCallback(UnityAction<object, int> itemDroppedCallback) {
        this.itemDroppedCallback.RemoveAllListeners();
        this.itemDroppedCallback.AddListener(itemDroppedCallback);
    }
    public void SetItemDroppedOutCallback(UnityAction<object, int> itemDroppedOutCallback) {
        this.itemDroppedOutCallback.RemoveAllListeners();
        this.itemDroppedOutCallback.AddListener(itemDroppedOutCallback);
    }
    #endregion
}

[System.Serializable]
public class SlotItemDropEvent : UnityEvent<IDragParentItem> { }

public class ItemDroppedCallback : UnityEvent<object, int> { }
public class ItemDroppedOutCallback : UnityEvent<object, int> { }
