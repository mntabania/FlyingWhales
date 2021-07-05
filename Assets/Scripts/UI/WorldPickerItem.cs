using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using EZObjectPools;
using TMPro;
using UnityEngine.UI;

public class WorldPickerItem : MonoBehaviour {
    public WorldSettingsData.World_Type worldType;
    [TextArea(3, 10)]
    public string description;
    public Toggle toggle;
    public GameObject cover;
    public bool isScenario;
    
    public void OnHoverEnter() {
        WorldSettings.Instance.OnHoverEnterWorldPicker(this);
    }
    public void OnHoverExit() {
        WorldSettings.Instance.OnHoverExitWorldPicker(this);
    }
    public void OnToggle(bool state) {
        WorldSettings.Instance.OnToggleWorldPicker(this, state);
    }
    public void Disable() {
        toggle.interactable = false;
        cover.gameObject.SetActive(true);
    }
    public void Enable() {
        toggle.interactable = true;
        cover.gameObject.SetActive(false);
    }    
}
