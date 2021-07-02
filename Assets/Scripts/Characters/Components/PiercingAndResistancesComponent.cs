using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;
public class PiercingAndResistancesComponent : CharacterComponent {
    /// <summary>
    /// This is the computed value of basePiercing * piercingMultiplier
    /// </summary>
    public float piercingPower { get; private set; }
    public Dictionary<RESISTANCE, float> resistances { get; private set; }
    public Dictionary<RESISTANCE, float> resistancesMultipliers { get; private set; }
    public float piercingMultiplier { get; private set; }
    public float basePiercing { get; private set; }
    
    public PiercingAndResistancesComponent() {
        resistances = new Dictionary<RESISTANCE, float>();
        resistancesMultipliers = new Dictionary<RESISTANCE, float>();
    }
    public PiercingAndResistancesComponent(SaveDataPiercingAndResistancesComponent data) {
        piercingPower = data.piercingPower;
        resistances = data.resistances;
        resistancesMultipliers = data.resistancesMultipliers;
        piercingMultiplier = data.piercingMultiplier;
        basePiercing = data.basePiercing;
    }

    #region Piercing
    public void AdjustBasePiercing(float p_amount) {
        basePiercing += p_amount;
        UpdatePiercing();
    }
    public void SetBasePiercing(float p_amount) {
        basePiercing = p_amount;
        UpdatePiercing();
    }
    public void AdjustPiercingMultiplier(float p_amount) {
        piercingMultiplier += p_amount;
        UpdatePiercing();
    }
    private void UpdatePiercing() {
        piercingPower = basePiercing + (basePiercing * (piercingMultiplier / 100f));
        piercingPower = Mathf.Round(piercingPower);
        Messenger.Broadcast(UISignals.UPDATE_PIERCING_AND_RESISTANCE_INFO, owner);
    }
    #endregion

    #region Resistances
    public void AdjustResistance(RESISTANCE p_resistance, float p_value, bool shouldBroadcastSignal = true) {
        if (!resistances.ContainsKey(p_resistance)) {
            resistances.Add(p_resistance, 0f);
        }
        resistances[p_resistance] += p_value;
#if DEBUG_LOG
        Debug.Log($"Adjusted base resistance {p_resistance.ToString()} of {owner.name} by {p_value.ToString()}. New value is: {resistances[p_resistance].ToString()}");
#endif
        if (shouldBroadcastSignal) {
            Messenger.Broadcast(UISignals.UPDATE_PIERCING_AND_RESISTANCE_INFO, owner);
        }
    }
    public void AdjustAllResistances(float p_value) {
        RESISTANCE[] resistances = CollectionUtilities.GetEnumValues<RESISTANCE>();
        for (int i = 0; i < resistances.Length; i++) {
            RESISTANCE r = resistances[i];
            if (r != RESISTANCE.None) {
                AdjustResistance(r, p_value, shouldBroadcastSignal: false);
            }
        }
        Messenger.Broadcast(UISignals.UPDATE_PIERCING_AND_RESISTANCE_INFO, owner);
    }
    public void SetResistance(RESISTANCE p_resistance, float p_value) {
        if (!resistances.ContainsKey(p_resistance)) {
            resistances.Add(p_resistance, 0f);
        }
        resistances[p_resistance] = p_value;
#if DEBUG_LOG
        Debug.Log($"Set base resistance {p_resistance.ToString()} of {owner.name} to {p_value.ToString()}");
#endif
        Messenger.Broadcast(UISignals.UPDATE_PIERCING_AND_RESISTANCE_INFO, owner);
    }
    public float GetResistanceValue(RESISTANCE p_resistance) {
        if (resistances.ContainsKey(p_resistance)) {
            float baseResistance = resistances[p_resistance];
            float multiplier = 1f;
            if (resistancesMultipliers.ContainsKey(p_resistance)) {
                multiplier = resistancesMultipliers[p_resistance];
                multiplier = Mathf.Max(multiplier, 1);
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
    public float piercingMultiplier;
    public float basePiercing;

    #region Overrides
    public override void Save(PiercingAndResistancesComponent data) {
        piercingPower = data.piercingPower;
        resistances = new Dictionary<RESISTANCE, float>();
        foreach (var kvp in data.resistances) {
            resistances.Add(kvp.Key, kvp.Value);
        }
        resistancesMultipliers = new Dictionary<RESISTANCE, float>();
        foreach (var kvp in data.resistancesMultipliers) {
            resistancesMultipliers.Add(kvp.Key, kvp.Value);
        }
        piercingMultiplier = data.piercingMultiplier;
        basePiercing = data.basePiercing;
    }

    public override PiercingAndResistancesComponent Load() {
        PiercingAndResistancesComponent component = new PiercingAndResistancesComponent(this);
        return component;
    }
    #endregion
}