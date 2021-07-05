using System;
using TMPro;
using UnityEngine;

public class ChanceItem : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private TMP_InputField inputField;

    private CHANCE_TYPE _chanceType;

    public void Initialize(CHANCE_TYPE p_chanceType) {
        _chanceType = p_chanceType;
        nameLbl.text = p_chanceType.ToString();
        inputField.onValueChanged.AddListener(OnValueChanged);
        UpdateChanceText();
    }
    private void OnValueChanged(string p_newValue) {
        int newValue = Int32.Parse(p_newValue);
        ChanceData.SetChance(_chanceType, newValue);
    }
    private void UpdateChanceText() {
        inputField.SetTextWithoutNotify(ChanceData.GetChance(_chanceType).ToString());
    }


}
