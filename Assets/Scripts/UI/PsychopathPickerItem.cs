using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using TMPro;

public class PsychopathPickerItem : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI mainLbl;
    [SerializeField] private Toggle toggle;

    private Action<string, bool> onToggleNameplate;
    private Action<string> onHoverEnter;
    private Action<string> onHoverExit;

    private string str;

    public void SetObject(string o) {
        str = o;
        name = o;
        mainLbl.text = o;
    }
    public void SetToggleAction(Action<string, bool> onToggleNameplate) {
        this.onToggleNameplate = onToggleNameplate;
    }
    public void SetOnHoverEnterAction(Action<string> onHoverEnter) {
        this.onHoverEnter = onHoverEnter;
    }
    public void SetOnHoverExitAction(Action<string> onHoverExit) {
        this.onHoverExit = onHoverExit;
    }
    public void SetToggleGroup(ToggleGroup group) {
        toggle.group = group;
    }
    public void OnToggle(bool isOn) {
        onToggleNameplate(str, isOn);
    }
    public void OnHoverEnter() {
        if(onHoverEnter != null) {
            onHoverEnter(str);
        }
    }
    public void OnHoverExit() {
        if (onHoverExit != null) {
            onHoverExit(str);
        }
    }
}
