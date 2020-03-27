using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemListUI : PopupMenuBase {

    [SerializeField] private Toggle itemsToggle;
    public override void Open() {
        base.Open();
        itemsToggle.isOn = true;
    }
    public override void Close() {
        itemsToggle.isOn = false;
        base.Close();
    }
}
