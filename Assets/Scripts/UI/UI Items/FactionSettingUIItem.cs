using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FactionSettingUIItem : MonoBehaviour {

    public static System.Action<FactionSetting, FactionSettingUIItem> onClickMinus;
    public static System.Action<FactionSetting, string> onChangeName;
    public static System.Action<FactionSetting, FactionSettingUIItem> onClickRandomizeName;
    public static System.Action<FactionSetting, string> onChangeFactionType;
    public static System.Action<FactionSetting> onClickEditVillages;
    public static System.Action<FactionSetting> onHoverOverEditVillages;
    public static System.Action<FactionSetting> onHoverOutEditVillages;
    
    public Button btnMinus;
    public Image imgFactionEmblem;
    public TMP_InputField inputFieldName;
    public Button btnRandomizeName;
    public TMP_Dropdown dropDownFactionType;
    public TextMeshProUGUI txtVillageCount;
    public Button btnEditVillages;

    public HoverHandler hoverHandlerEditVillages;

    private FactionSetting _factionSetting;
    
    private void OnEnable() {
        btnMinus.onClick.AddListener(OnClickMinus);
        inputFieldName.onValueChanged.AddListener(OnChangeName);
        btnRandomizeName.onClick.AddListener(OnClickRandomizeName);
        dropDownFactionType.onValueChanged.AddListener(OnChangeFactionType);
        btnEditVillages.onClick.AddListener(OnClickEditVillages);
        hoverHandlerEditVillages.AddOnHoverOverAction(OnHoverOverEditVillages);
        hoverHandlerEditVillages.AddOnHoverOutAction(OnHoverOutEditVillages);
    }
    private void OnDisable() {
        btnMinus.onClick.RemoveListener(OnClickMinus);
        inputFieldName.onValueChanged.RemoveListener(OnChangeName);
        btnRandomizeName.onClick.RemoveListener(OnClickRandomizeName);
        dropDownFactionType.onValueChanged.RemoveListener(OnChangeFactionType);
        btnEditVillages.onClick.RemoveListener(OnClickEditVillages);
        hoverHandlerEditVillages.RemoveOnHoverOverAction(OnHoverOverEditVillages);
        hoverHandlerEditVillages.RemoveOnHoverOutAction(OnHoverOutEditVillages);
    }

    public void Initialize(List<string> p_choices) {
        dropDownFactionType.ClearOptions();
        dropDownFactionType.AddOptions(p_choices);
    }
    public void Reset() {
        dropDownFactionType.SetValueWithoutNotify(0);
    }
    public void SetItemDetails(FactionSetting p_factionSetting) {
        _factionSetting = p_factionSetting;
        imgFactionEmblem.sprite = p_factionSetting.factionEmblem;
        UpdateName(p_factionSetting.name);
        dropDownFactionType.value = dropDownFactionType.GetDropdownOptionIndex(p_factionSetting.factionTypeString);
        txtVillageCount.text = p_factionSetting.villageSettings.Count.ToString();
    }
    public void SetMinusBtnState(bool p_state) {
        btnMinus.gameObject.SetActive(p_state);
    }
    public void UpdateName(string p_name) {
        inputFieldName.SetTextWithoutNotify(p_name);
    }

    private void OnClickMinus() {
        onClickMinus?.Invoke(_factionSetting, this);
    }
    private void OnChangeName(string p_newValue) {
        onChangeName?.Invoke(_factionSetting, p_newValue);
    }
    private void OnClickRandomizeName() {
        onClickRandomizeName?.Invoke(_factionSetting, this);
    }
    private void OnChangeFactionType(int p_index) {
        string chosen = UtilityScripts.Utilities.NotNormalizedConversionStringToEnum(dropDownFactionType.options[p_index].text);
        onChangeFactionType?.Invoke(_factionSetting, chosen);
    }
    private void OnClickEditVillages() {
        onClickEditVillages?.Invoke(_factionSetting);
    }
    private void OnHoverOverEditVillages() {
        onHoverOverEditVillages?.Invoke(_factionSetting);
    }
    private void OnHoverOutEditVillages() {
        onHoverOutEditVillages?.Invoke(_factionSetting);
    }
}
