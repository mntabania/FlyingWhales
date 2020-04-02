using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponTypeButton : MonoBehaviour {
    public Text buttonText;
    public string panelName;
    public string categoryName;

    public void SetCurrentlySelectedButton() {
        if(panelName == "skill") {
            SkillPanelUI.Instance.currentSelectedWeaponTypeButton = this;
        }else if (panelName == "class") {
            //if(categoryName == "weapon") {
            //    ClassPanelUI.Instance.currentSelectedWeaponButton = this;
            //} else if (categoryName == "armor") {
            //    ClassPanelUI.Instance.currentSelectedArmorButton = this;
            //} else if (categoryName == "accessory") {
            //    ClassPanelUI.Instance.currentSelectedAccessoryButton = this;
            //}
        }
    }
}
