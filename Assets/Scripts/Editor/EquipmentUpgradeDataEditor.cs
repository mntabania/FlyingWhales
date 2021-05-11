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
    private CONCRETE_RESOURCES m_concreteResource;
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
        DisplayCompatibleClasses();
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

    void DisplayCompatibleClasses() {
        DisplayEnumList(data.compatibleClasses, "Classes That Can use this Equip");
        EditorGUILayout.Space();
    }

    void DisplayResourcesEditor() {
        m_resourceType = data.resourceType = (RESOURCE)EditorGUILayout.EnumPopup("Any of resource type", m_resourceType);
        EditorGUILayout.Space();
        if (m_isTypeResourcesNone) {
            m_concreteResource = data.specificResource = (CONCRETE_RESOURCES)EditorGUILayout.EnumPopup("Specific Resource", m_concreteResource);
            EditorGUILayout.Space();
        }
    }

    void DisplayUpgradeBonus() {
        if (true) {
            DisplayEnumList(data.equipmentUpgradeData.bonuses, "Bonus stats");
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (data.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Increased_Piercing)) {
                m_additionalPiercing = data.equipmentUpgradeData.AdditionalPiercing = EditorGUILayout.FloatField("additional Piercing", data.equipmentUpgradeData.AdditionalPiercing);
                EditorGUILayout.Space();
            }
            if (data.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Max_HP_Percentage)) {
                m_additionalMaxHPPercentage = data.equipmentUpgradeData.AdditionalmaxHPPercentage = EditorGUILayout.FloatField("additional Max HP(%)", data.equipmentUpgradeData.AdditionalmaxHPPercentage);
                EditorGUILayout.Space();
            }
            if (data.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Max_HP_Actual)) {
                m_additionalMaxHPActual = data.equipmentUpgradeData.AdditionalmaxHPActual = EditorGUILayout.FloatField("additional Max HP(actual)", data.equipmentUpgradeData.AdditionalmaxHPActual);
                EditorGUILayout.Space();
            }
            if (data.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Atk_Percentage)) {
                m_additionalAttackPercentage = data.equipmentUpgradeData.AdditionalAttackPercentage = EditorGUILayout.FloatField("additional Attack(%)", data.equipmentUpgradeData.AdditionalAttackPercentage);
                EditorGUILayout.Space();
            }
            if (data.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Atk_Actual)) {
                m_additionalAttackActual = data.equipmentUpgradeData.AdditionalAttackActual = EditorGUILayout.FloatField("additional Attack(actual)", data.equipmentUpgradeData.AdditionalAttackActual);
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
}
