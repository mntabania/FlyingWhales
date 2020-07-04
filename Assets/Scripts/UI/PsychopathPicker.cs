using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

public class PsychopathPicker : MonoBehaviour {

    [Header("Object Picker")]
    [SerializeField] private ScrollRect objectPickerScrollView;
    [SerializeField] private GameObject psychopathPickerItemPrefab;
    [SerializeField] private GameObject cover;
    [SerializeField] private Button closeBtn;
    [SerializeField] private ToggleGroup toggleGroup;

    private System.Action<string> onConfirmAction;
    private string pickedString;

    public void ShowPicker(List<string> items, Action<string> onConfirmAction, Action<string> onHoverEnterAction, Action<string> onHoverExitAction) {
        UtilityScripts.Utilities.DestroyChildren(objectPickerScrollView.content);
        this.onConfirmAction = onConfirmAction;
        for (int i = 0; i < items.Count; i++) {
            string str = items[i];
            GameObject go = Instantiate(psychopathPickerItemPrefab, objectPickerScrollView.content);
            PsychopathPickerItem item = go.GetComponent<PsychopathPickerItem>();
            item.SetObject(str);
            item.SetToggleGroup(toggleGroup);
            item.SetToggleAction(OnPickSkill);
            item.SetOnHoverEnterAction(onHoverEnterAction);
            item.SetOnHoverExitAction(onHoverExitAction);
        }
        Open();
    }
    private void OnPickSkill(string str, bool isOn) {
        if (isOn) {
            pickedString = str;
            OnClickConfirm();
        }
        //else {
        //    if (pickedSkill == skillData) {
        //        pickedSkill = null;
        //    }
        //}
        //UpdateConfirmBtnState();
    }
    public void Open() {
        gameObject.SetActive(true);
    }
    public void Close() {
        gameObject.SetActive(false);
    }
    public void OnClickConfirm() {
        onConfirmAction(pickedString);
        Close();
    }
}