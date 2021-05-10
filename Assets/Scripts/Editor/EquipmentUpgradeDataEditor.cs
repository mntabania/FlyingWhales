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
    private ELEMENTAL_TYPE m_elementalBonus;
    private EQUIPMENT_SLAYER_BONUS m_equipmentSlayerBonus;
    private EQUIPMENT_WARD_BONUS m_equipmentWardBonus;
    void OnEnable() {
        data = (EquipmentData)target;
        AssetDatabase.Refresh();
    }

    public override void OnInspectorGUI() {
        EditorGUILayout.Space();
        DrawDefaultInspector();
        EditorGUILayout.Space();
        serializedObject.ApplyModifiedProperties();
        EditorGUILayout.Space();
        EditorGUILayout.TextArea("UPGRADE Stats/Skill Bonus");
        DisplayUpgradeBonus();
        serializedObject.ApplyModifiedProperties();
        if (GUI.changed) {
            EditorUtility.SetDirty(target);
        }
    }

    private void OnDisable() {
        //AssetDatabase.SaveAssets();
    }

    void DisplayUpgradeBonus() {
        if (true) {
            DisplayEnumList(data.equipmentUpgradeData.bonuses, "Bonus stats");
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (data.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Increased_Piercing)) {
                m_additionalPiercing = data.equipmentUpgradeData.AdditionalPiercing = EditorGUILayout.FloatField("additional Piercing", m_additionalPiercing);
                EditorGUILayout.Space();
            }
            if (data.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Max_HP_Percentage)) {
                m_additionalMaxHPPercentage = data.equipmentUpgradeData.AdditionalmaxHPPercentage = EditorGUILayout.FloatField("additional Max HP(%)", m_additionalMaxHPPercentage);
                EditorGUILayout.Space();
            }
            if (data.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Max_HP_Actual)) {
                m_additionalMaxHPActual = data.equipmentUpgradeData.AdditionalmaxHPActual = EditorGUILayout.FloatField("additional Max HP(actual)", m_additionalMaxHPActual);
                EditorGUILayout.Space();
            }
            if (data.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Atk_Percentage)) {
                m_additionalAttackPercentage = data.equipmentUpgradeData.AdditionalAttackPercentage = EditorGUILayout.FloatField("additional Attack(%)", m_additionalAttackPercentage);
                EditorGUILayout.Space();
            }
            if (data.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Atk_Actual)) {
                m_additionalAttackActual = data.equipmentUpgradeData.AdditionalAttackActual = EditorGUILayout.FloatField("additional Attack(actual)", m_additionalAttackActual);
                EditorGUILayout.Space();
            }
            if (data.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Attack_Element)) {
                m_elementalBonus = data.equipmentUpgradeData.elementAttackBonus = (ELEMENTAL_TYPE)EditorGUILayout.EnumFlagsField("Element Bonus", m_elementalBonus);
                EditorGUILayout.Space();
            }
            if (data.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Slayer_Bonus)) {
                m_equipmentSlayerBonus = data.equipmentUpgradeData.slayerBonus = (EQUIPMENT_SLAYER_BONUS)EditorGUILayout.EnumFlagsField("Slayer Bonus", m_equipmentSlayerBonus);
                EditorGUILayout.Space();
            }
            if (data.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Ward_Bonus)) {
                m_equipmentWardBonus = data.equipmentUpgradeData.wardBonus = (EQUIPMENT_WARD_BONUS)EditorGUILayout.EnumFlagsField("Ward Bonus", m_equipmentWardBonus);
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
}
