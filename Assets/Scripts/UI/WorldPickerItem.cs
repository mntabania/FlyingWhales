using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using EZObjectPools;
using TMPro;
using UnityEngine.UI;

public class WorldPickerItem : MonoBehaviour {
    public WorldSettings.World worldType;
    [TextArea(3, 10)]
    public string description;
    
    public void OnHoverEnter() {
        WorldSettings.Instance.OnHoverEnterWorldPicker(this);
    }
    public void OnHoverExit() {
        WorldSettings.Instance.OnHoverExitWorldPicker(this);
    }
    public void OnToggle(bool state) {
        WorldSettings.Instance.OnToggleWorldPicker(this, state);
    }
}
