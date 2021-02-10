using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiercingAndResistancesComponent : CharacterComponent {
    public float piercingPower { get; private set; }
    public Dictionary<RESISTANCE, float> resistances { get; private set; }

    public PiercingAndResistancesComponent() {
        resistances = new Dictionary<RESISTANCE, float>();
    }
    public PiercingAndResistancesComponent(SaveDataPiercingAndResistancesComponent data) {
        piercingPower = data.piercingPower;
        resistances = data.resistances;
    }

    #region Resistances
    public void AdjustResistance(RESISTANCE p_resistance, float p_value) {
        if (!resistances.ContainsKey(p_resistance)) {
            resistances.Add(p_resistance, 0f);
        }
        resistances[p_resistance] += p_value;
    }
    public float GetResistanceValue(RESISTANCE p_resistance) {
        if (resistances.ContainsKey(p_resistance)) {
            return resistances[p_resistance];
        }
        return 0f;
    }
    public float GetResistanceValue(ELEMENTAL_TYPE p_element) {
        RESISTANCE resistance = p_element.GetResistance();
        if (resistances.ContainsKey(resistance)) {
            return resistances[resistance];
        }
        return 0f;
    }
    public void ModifyValueByResistance(ref int p_value, ELEMENTAL_TYPE p_element, float piercingPower) {
        float resistanceValue = GetResistanceValue(p_element);

        float finalResistanceValue = resistanceValue - piercingPower;
        if(finalResistanceValue < 0f) {
            finalResistanceValue = 0f;
        } else if (finalResistanceValue > 100f) {
            finalResistanceValue = 100f;
        }
        finalResistanceValue = finalResistanceValue / 100f;

        int value = p_value;
        int finalValue = value - Mathf.RoundToInt(value * finalResistanceValue);
        p_value = finalValue;
    }
    #endregion
}

[System.Serializable]
public class SaveDataPiercingAndResistancesComponent : SaveData<PiercingAndResistancesComponent> {
    public float piercingPower;
    public Dictionary<RESISTANCE, float> resistances;

    #region Overrides
    public override void Save(PiercingAndResistancesComponent data) {
        piercingPower = data.piercingPower;
        resistances = data.resistances;
    }

    public override PiercingAndResistancesComponent Load() {
        PiercingAndResistancesComponent component = new PiercingAndResistancesComponent(this);
        return component;
    }
    #endregion
}