using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VillageSettingUIItem : MonoBehaviour {

    public static System.Action<VillageSettingUIItem> onClickMinus;
    public static System.Action<VillageSettingUIItem, string> onChangeName;
    public static System.Action<VillageSettingUIItem> onClickRandomizeName;
    public static System.Action<VillageSettingUIItem, VILLAGE_SIZE> onChangeVillageSize;
    
    public Button btnMinus;
    public TMP_InputField inputFieldName;
    public Button btnRandomizeName;
    public TMP_Dropdown dropDownVillageSize;
    private void OnEnable() {
        btnMinus.onClick.AddListener(OnClickMinus);
        inputFieldName.onValueChanged.AddListener(OnChangeFactionName);
        btnRandomizeName.onClick.AddListener(OnClickRandomizeName);
        dropDownVillageSize.onValueChanged.AddListener(OnChangeVillageSize);
    }
    private void OnDisable() {
        btnMinus.onClick.RemoveListener(OnClickMinus);
        inputFieldName.onValueChanged.RemoveListener(OnChangeFactionName);
        btnRandomizeName.onClick.RemoveListener(OnClickRandomizeName);
        dropDownVillageSize.onValueChanged.RemoveListener(OnChangeVillageSize);
    }
    
    public void Initialize(List<string> p_villageSizeChoices) {
        dropDownVillageSize.ClearOptions();
        dropDownVillageSize.AddOptions(p_villageSizeChoices);
    }
    public void SetItemDetails(VillageSetting p_villageSettings) {
        inputFieldName.SetTextWithoutNotify(p_villageSettings.villageName);
        dropDownVillageSize.SetValueWithoutNotify(dropDownVillageSize.GetDropdownOptionIndex(UtilityScripts.Utilities.NotNormalizedConversionEnumToString(p_villageSettings.villageSize.ToString())));
    }
    public void SetMinusBtnState(bool p_state) {
        btnMinus.gameObject.SetActive(p_state);
    }

    private void OnClickMinus() {
        onClickMinus?.Invoke(this);
    }
    private void OnChangeFactionName(string p_newName) {
        onChangeName?.Invoke(this, p_newName);
    }
    private void OnClickRandomizeName() {
        onClickRandomizeName?.Invoke(this);
    }
    private void OnChangeVillageSize(int p_index) {
        string chosen = UtilityScripts.Utilities.NotNormalizedConversionStringToEnum(dropDownVillageSize.options[p_index].text);
        VILLAGE_SIZE villageSize = (VILLAGE_SIZE)System.Enum.Parse(typeof(VILLAGE_SIZE), chosen, true);
        onChangeVillageSize?.Invoke(this, villageSize);
    }
}
