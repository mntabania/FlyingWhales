using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiercingAndResistancesComponent : CharacterComponent {
    public float piercingPower { get; private set; }
    public Dictionary<RESISTANCE, float> resistances { get; private set; }
    public Dictionary<RESISTANCE, float> resistancesMultipliers { get; private set; }

    public PiercingAndResistancesComponent() {
        resistances = new Dictionary<RESISTANCE, float>();
        resistancesMultipliers = new Dictionary<RESISTANCE, float>();
    }
    public PiercingAndResistancesComponent(SaveDataPiercingAndResistancesComponent data) {
        piercingPower = data.piercingPower;
        resistances = data.resistances;
        resistancesMultipliers = data.resistancesMultipliers;
    }

    #region Piercing
    public void AdjustPiercing(float p_amount) {
        piercingPower += p_amount;
        Messenger.Broadcast(UISignals.UPDATE_PIERCING_AND_RESISTANCE_INFO, owner);
    }
    public void SetPiercing(float p_amount) {
        piercingPower = p_amount;
        Messenger.Broadcast(UISignals.UPDATE_PIERCING_AND_RESISTANCE_INFO, owner);
    }
    #endregion

    #region Resistances
    public void AdjustResistance(RESISTANCE p_resistance, float p_value) {
        if (!resistances.ContainsKey(p_resistance)) {
            resistances.Add(p_resistance, 0f);
        }
        resistances[p_resistance] += p_value;
        Messenger.Broadcast(UISignals.UPDATE_PIERCING_AND_RESISTANCE_INFO, owner);
    }
    public void SetResistance(RESISTANCE p_resistance, float p_value) {
        if (!resistances.ContainsKey(p_resistance)) {
            resistances.Add(p_resistance, 0f);
        }
        resistances[p_resistance] = p_value;
        Messenger.Broadcast(UISignals.UPDATE_PIERCING_AND_RESISTANCE_INFO, owner);
    }
    public float GetResistanceValue(RESISTANCE p_resistance) {
        if (resistances.ContainsKey(p_resistance)) {
            float baseResistance = resistances[p_resistance];
            float multiplier = 1f;
            if (resistancesMultipliers.ContainsKey(p_resistance)) {
                multiplier = resistancesMultipliers[p_resistance];
            }
            return baseResistance * multiplier;
        }
        return 0f;
    }
    public float GetResistanceValue(ELEMENTAL_TYPE p_element) {
        RESISTANCE resistance = p_element.GetResistance();
        return GetResistanceValue(resistance);
    }
    public void ModifyValueByResistance(ref int p_value, ELEMENTAL_TYPE p_element, float piercingPower) {
        float resistanceValue = GetResistanceValue(p_element);
        CombatManager.ModifyValueByPiercingAndResistance(ref p_value, piercingPower, resistanceValue);
    }
    #endregion

    #region Modifiers
    public void AdjustResistanceMultiplier(RESISTANCE p_resistance, float p_value) {
        if (!resistancesMultipliers.ContainsKey(p_resistance)) {
            resistancesMultipliers.Add(p_resistance, 0f);
        }
        resistancesMultipliers[p_resistance] += p_value;
        Messenger.Broadcast(UISignals.UPDATE_PIERCING_AND_RESISTANCE_INFO, owner);
    }
    #endregion
}

[System.Serializable]
public class SaveDataPiercingAndResistancesComponent : SaveData<PiercingAndResistancesComponent> {
    public float piercingPower;
    public Dictionary<RESISTANCE, float> resistances;
    public Dictionary<RESISTANCE, float> resistancesMultipliers;

    #region Overrides
    public override void Save(PiercingAndResistancesComponent data) {
        piercingPower = data.piercingPower;
        resistances = data.resistances;
        resistancesMultipliers = data.resistancesMultipliers;
    }

    public override PiercingAndResistancesComponent Load() {
        PiercingAndResistancesComponent component = new PiercingAndResistancesComponent(this);
        return component;
    }
    #endregion
}