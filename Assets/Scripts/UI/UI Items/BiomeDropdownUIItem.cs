using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BiomeDropdownUIItem : MonoBehaviour {

    public static System.Action<BiomeDropdownUIItem, string> onChooseBiome;
    public static System.Action<BiomeDropdownUIItem, string> onClickMinus;
    
    public TMP_Dropdown dropdownBiome;
    public Button btnMinus;
    
    private void OnEnable() {
        dropdownBiome.onValueChanged.AddListener(OnChooseBiome);
        btnMinus.onClick.AddListener(OnClickMinus);
    }
    private void OnDisable() {
        dropdownBiome.onValueChanged.RemoveListener(OnChooseBiome);
        btnMinus.onClick.RemoveListener(OnClickMinus);
    }
    public void Initialize(List<string> p_biomes) {
        dropdownBiome.ClearOptions();
        dropdownBiome.AddOptions(p_biomes);
    }
    public void Reset() {
        dropdownBiome.SetValueWithoutNotify(0);
    }
    public void SetMinusBtnState(bool p_state) {
        btnMinus.gameObject.SetActive(p_state);
    }
    private void OnChooseBiome(int p_index) {
        string chosen = dropdownBiome.options[p_index].text;
        onChooseBiome?.Invoke(this, chosen);
    }
    private void OnClickMinus() {
        string chosen = dropdownBiome.options[dropdownBiome.value].text;
        onClickMinus?.Invoke(this, chosen);
    }
    
}
