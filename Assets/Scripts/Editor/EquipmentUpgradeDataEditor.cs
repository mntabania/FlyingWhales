using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(EquipmentData), true)]
public class EquipmentUpgradeDataEditor : Editor {

    EquipmentData data;

    private float m_additionalPiercing;
    private float m_additionalMaxHPPercentage;
    private float m_additionalMaxHPActual;
    private float m_additionalAttackPercentage;
    private float m_additionalAttackActual;
    private float m_additionalIntPercentage;
    private float m_additionalIntActual;
    private float m_additionalCritrate;
    private float m_resitanceBonusActualValue;
    private ELEMENTAL_TYPE m_elementalBonus;
    private EQUIPMENT_SLAYER_BONUS m_equipmentSlayerBonus;
    private EQUIPMENT_WARD_BONUS m_equipmentWardBonus;
    private RESOURCE m_resourceType;

    private bool m_isTypeResourcesNone;
    void OnEnable() {
        data = (EquipmentData)target;
        AssetDatabase.Refresh();
    }

    public override void OnInspectorGUI() {
        DisplayResourcesEditor();
        EditorGUILayout.Space();
        DrawDefaultInspector();
        EditorGUILayout.Space();
        serializedObject.ApplyModifiedProperties();
        EditorGUILayout.Space();
        EditorGUILayout.TextArea("UPGRADE Stats/Skill Bonus");
        DisplayUpgradeBonus();
        serializedObject.ApplyModifiedProperties();
        if (data.resourceType == RESOURCE.NONE) {
            m_isTypeResourcesNone = true;
        } else {
            m_isTypeResourcesNone = false;
        }
        if (GUI.changed) {
            EditorUtility.SetDirty(target);
        }
    }

    private void OnDisable() {
        //AssetDatabase.SaveAssets();
    }

    void DisplayResourcesEditor() {
        m_resourceType = data.resourceType = (RESOURCE)EditorGUILayout.EnumPopup("Any of resource type", data.resourceType);
        EditorGUILayout.Space();
        if (m_isTypeResourcesNone) {
            DisplayResourceList(data.specificResource, "Reource List (OR)");
            EditorGUILayout.Space();
        }
    }

    void DisplayUpgradeBonus() {
        if (true) {
            DisplayEnumList(data.equipmentUpgradeData.bonuses, "Bonus stats");
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (data.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Increased_Piercing)) {
                m_additionalPiercing = data.equipmentUpgradeData.AdditionalPiercing = EditorGUILayout.FloatField("Additional Piercing", data.equipmentUpgradeData.AdditionalPiercing);
                EditorGUILayout.Space();
            }
            if (data.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Max_HP_Percentage)) {
                m_additionalMaxHPPercentage = data.equipmentUpgradeData.AdditionalMaxHPPercentage = EditorGUILayout.FloatField("Additional Max HP(%)", data.equipmentUpgradeData.AdditionalMaxHPPercentage);
                EditorGUILayout.Space();
            }
            if (data.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Max_HP_Actual)) {
                m_additionalMaxHPActual = data.equipmentUpgradeData.AdditionalMaxHPActual = EditorGUILayout.IntField("Additional Max HP(actual)", data.equipmentUpgradeData.AdditionalMaxHPActual);
                EditorGUILayout.Space();
            }
            if (data.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Str_Percentage)) {
                m_additionalAttackPercentage = data.equipmentUpgradeData.AdditionalAttackPercentage = EditorGUILayout.FloatField("Additional Strength(%)", data.equipmentUpgradeData.AdditionalAttackPercentage);
                EditorGUILayout.Space();
            }
            if (data.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Str_Actual)) {
                m_additionalAttackActual = data.equipmentUpgradeData.AdditionalAttackActual = EditorGUILayout.IntField("Additional Strength(actual)", data.equipmentUpgradeData.AdditionalAttackActual);
                EditorGUILayout.Space();
            }
            if (data.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Int_Actual)) {
                m_additionalIntActual = data.equipmentUpgradeData.AdditionalIntActual = EditorGUILayout.IntField("Additional Int(actual)", data.equipmentUpgradeData.AdditionalIntActual);
                EditorGUILayout.Space();
            }
            if (data.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Int_Percentage)) {
                m_additionalIntPercentage = data.equipmentUpgradeData.AdditionalIntPercentage = EditorGUILayout.FloatField("Additional Int(percentage)", data.equipmentUpgradeData.AdditionalIntPercentage);
                EditorGUILayout.Space();
            }
            if (data.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Crit_Rate_Actual)) {
                m_additionalCritrate = data.equipmentUpgradeData.AdditionalCritRate = EditorGUILayout.IntField("Additional Crit rate", data.equipmentUpgradeData.AdditionalCritRate);
                EditorGUILayout.Space();
            }
            if (data.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Increased_3_Random_Resistance) || data.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Increased_4_Random_Resistance) || data.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Increased_5_Random_Resistance)) {
                m_resitanceBonusActualValue = data.equipmentUpgradeData.additionalResistanceBonus = EditorGUILayout.FloatField("Resistance Bonus(actual)", data.equipmentUpgradeData.additionalResistanceBonus);
                EditorGUILayout.Space();
            }
            if (data.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Attack_Element)) {
                m_elementalBonus = data.equipmentUpgradeData.elementAttackBonus = (ELEMENTAL_TYPE)EditorGUILayout.EnumPopup("Element Bonus", data.equipmentUpgradeData.elementAttackBonus);
                EditorGUILayout.Space();
            }
            if (data.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Slayer_Bonus)) {
                m_equipmentSlayerBonus = data.equipmentUpgradeData.slayerBonus = (EQUIPMENT_SLAYER_BONUS)EditorGUILayout.EnumPopup("Slayer Bonus", m_equipmentSlayerBonus);
                EditorGUILayout.Space();
            }
            if (data.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Ward_Bonus)) {
                m_equipmentWardBonus = data.equipmentUpgradeData.wardBonus = (EQUIPMENT_WARD_BONUS)EditorGUILayout.EnumPopup("Ward Bonus", data.equipmentUpgradeData.wardBonus);
                EditorGUILayout.Space();
            }
        }
    }

    public void DisplayEnumList(List<EQUIPMENT_BONUS> listInt, string caption) {
        var list = listInt;
        int newCount = Mathf.Max(0, EditorGUILayout.IntField(caption, list.Count));
        while (newCount < list.Count)
            list.RemoveAt(list.Count - 1);
        while (newCount > list.Count)
            list.Add(0);

        for (int i = 0; i < list.Count; i++) {
            list[i] = (EQUIPMENT_BONUS)EditorGUILayout.EnumPopup((EQUIPMENT_BONUS)list[i]);
        }
    }

    public void DisplayEnumList(List<EQUIPMENT_CLASS_COMPATIBILITY> listInt, string caption) {
        var list = listInt;
        int newCount = Mathf.Max(0, EditorGUILayout.IntField(caption, list.Count));
        while (newCount < list.Count)
            list.RemoveAt(list.Count - 1);
        while (newCount > list.Count)
            list.Add(0);

        for (int i = 0; i < list.Count; i++) {
            list[i] = (EQUIPMENT_CLASS_COMPATIBILITY)EditorGUILayout.EnumPopup((EQUIPMENT_CLASS_COMPATIBILITY)list[i]);
        }
    }

    public void DisplayResourceList(List<CONCRETE_RESOURCES> listInt, string caption) {
        var list = listInt;
        int newCount = Mathf.Max(0, EditorGUILayout.IntField(caption, list.Count));
        while (newCount < list.Count)
            list.RemoveAt(list.Count - 1);
        while (newCount > list.Count)
            list.Add(0);

        for (int i = 0; i < list.Count; i++) {
            list[i] = (CONCRETE_RESOURCES)EditorGUILayout.EnumPopup((CONCRETE_RESOURCES)list[i]);
        }
    }
}
