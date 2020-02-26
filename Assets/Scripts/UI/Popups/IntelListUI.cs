using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntelListUI : PopupMenuBase {

    [SerializeField] private Toggle _intelToggle;
    public override void Open() {
        _intelToggle.isOn = true;
    }
    public override void Close() {
        _intelToggle.isOn = false;
    }
}
