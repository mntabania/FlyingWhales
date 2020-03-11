using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class CustomDropdownList : PopupMenuBase {
    [Header("Object Picker")]
    [SerializeField] private ScrollRect dropdownScrollView;
    [SerializeField] private GameObject dropdownListItemPrefab;
    private Action<string> onClickDropdownItem;

    public void ShowDropdown(List<string> items, Action<string> onClickDropdownItem, Func<string, bool> canChooseItem = null) {
        UtilityScripts.Utilities.DestroyChildren(dropdownScrollView.content);

        this.onClickDropdownItem = onClickDropdownItem;

        for (int i = 0; i < items.Count; i++) {
            string item = items[i];
            GameObject ddListItem = UIManager.Instance.InstantiateUIObject(dropdownListItemPrefab.name, dropdownScrollView.content);
            CustomDDListItem ddItem = ddListItem.GetComponent<CustomDDListItem>();
            ddItem.SetText(item);
            if (canChooseItem != null) {
                ddItem.SetCoverState(canChooseItem.Invoke(item) == false);
            }
            ddItem.SetClickAction(OnClickDropdownItem);
        }
        base.Open();
    }
    public void SetPosition(Vector3 position) {
        gameObject.transform.localPosition = position;
    }
    public void OnClickDropdownItem(CustomDDListItem item) {
        onClickDropdownItem.Invoke(item.itemText.text);
    }
}