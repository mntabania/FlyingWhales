using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SummonListUI : PopupMenuBase {

    [SerializeField] private Toggle summonsToggle;
    public override void Open() {
        base.Open();
        summonsToggle.isOn = true;
    }
    public override void Close() {
        summonsToggle.isOn = false;
        base.Close();
    }
}
