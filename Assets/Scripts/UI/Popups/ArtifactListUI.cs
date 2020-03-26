using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArtifactListUI : PopupMenuBase {

    [SerializeField] private Toggle artifactsToggle;
    public override void Open() {
        base.Open();
        artifactsToggle.isOn = true;
    }
    public override void Close() {
        artifactsToggle.isOn = false;
        base.Close();
    }
}
