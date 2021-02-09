using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(PlayerSkillData))]
public class SkillUpgradeDataEditor : Editor {

    PlayerSkillData data;

    bool foldUnlockingRequirements = true;
    bool foldUpgradeBonus = true;

    void OnEnable() {
        data = (PlayerSkillData)target;
    }

    public override void OnInspectorGUI() {

        DrawDefaultInspector();
        EditorGUILayout.Space();
        EditorGUILayout.TextArea("REQUIREMENTS FOR UNLOCKING");
        DisplayUnlockingRequirementBonus();
        EditorGUILayout.Space();
        EditorGUILayout.TextArea("UPGRADE BONUS STATS");
        DisplayUpgradeBonus();
    }

    void DisplayUpgradeBonus() {
        foldUpgradeBonus = EditorGUILayout.InspectorTitlebar(foldUpgradeBonus, this);
        if (foldUpgradeBonus) {
            DisplayEnumList(data.skillUpgradeData.bonuses, "Bonus stats");
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (data.skillUpgradeData.bonuses.Contains(UPGRADE_BONUS.Damage)) {
                DisplayFloatList(data.skillUpgradeData.additionalDamagePerLevel, "Damages per level");
                EditorGUILayout.Space();
            }
            if (data.skillUpgradeData.bonuses.Contains(UPGRADE_BONUS.Pierce)) {
                DisplayFloatList(data.skillUpgradeData.additionalPiercePerLevel, "Pierce(%) per level");
                EditorGUILayout.Space();
            }
            if (data.skillUpgradeData.bonuses.Contains(UPGRADE_BONUS.HP_Percentage)) {
                DisplayFloatList(data.skillUpgradeData.additionalHpPercentagePerLevel, "HP(%) per level");
                EditorGUILayout.Space();
            }
            if (data.skillUpgradeData.bonuses.Contains(UPGRADE_BONUS.HP_Actual_Amount)) {
                DisplayFloatList(data.skillUpgradeData.additionalHpValuePerLevel, "HP(amount) per level");
                EditorGUILayout.Space();
            }
            if (data.skillUpgradeData.bonuses.Contains(UPGRADE_BONUS.Atk_Percentage)) {
                DisplayFloatList(data.skillUpgradeData.additionalAttackPercentagePerLevel, "Atk(%) per level");
                EditorGUILayout.Space();
            }
            if (data.skillUpgradeData.bonuses.Contains(UPGRADE_BONUS.Atk_Actual_Amount)) {
                DisplayFloatList(data.skillUpgradeData.additionalAttackValuePerLevel, "Atk(amount) per level");
                EditorGUILayout.Space();
            }
            if (data.skillUpgradeData.bonuses.Contains(UPGRADE_BONUS.Mana_Received)) {
                DisplayFloatList(data.skillUpgradeData.additionalmanaReceivedPercentagePerLevel, "Mana(%) received per level");
                EditorGUILayout.Space();
            }
            if (data.skillUpgradeData.bonuses.Contains(UPGRADE_BONUS.Increase_Stats_Percentage)) {
                DisplayFloatList(data.skillUpgradeData.statsIncreasedPercentagePerLevel, "Stats Bonus(%) per level");
                EditorGUILayout.Space();
            }
            if (data.skillUpgradeData.bonuses.Contains(UPGRADE_BONUS.Duration)) {
                DisplayFloatList(data.skillUpgradeData.statsIncreasedPercentagePerLevel, "Duration Bonus(min) per level");
                EditorGUILayout.Space();
            }
        }
    }

    void DisplayUnlockingRequirementBonus() {
        foldUnlockingRequirements = EditorGUILayout.InspectorTitlebar(foldUnlockingRequirements, this);
        if (foldUnlockingRequirements) {
            DisplayEnumListReqruiements(data.requirementData.requirements, "Requirements");
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (data.requirementData.requirements.Contains(UNLOCKING_SKILL_REQUIREMENT.Archetype)) {
                data.requirementData.requiredArchetype = (PLAYER_ARCHETYPE)EditorGUILayout.EnumPopup("Required Archetype", data.requirementData.requiredArchetype);
                EditorGUILayout.Space();
            }
            if (data.requirementData.requirements.Contains(UNLOCKING_SKILL_REQUIREMENT.Skills)) {
                DisplaySkillListReqruiements(data.requirementData.requiredSkills, "required skill(s)");
                EditorGUILayout.Space();
            }
            if (data.requirementData.requirements.Contains(UNLOCKING_SKILL_REQUIREMENT.actions_count)) {
                data.requirementData.actionCount = EditorGUILayout.IntField("Action Count", data.requirementData.actionCount);
                EditorGUILayout.Space();
            }
            if (data.requirementData.requirements.Contains(UNLOCKING_SKILL_REQUIREMENT.affliction_count)) {
                data.requirementData.afflictionCount = EditorGUILayout.IntField("Affliction Count", data.requirementData.afflictionCount);
                EditorGUILayout.Space();
            }
            if (data.requirementData.requirements.Contains(UNLOCKING_SKILL_REQUIREMENT.spells_count)) {
                data.requirementData.spellsCount = EditorGUILayout.IntField("Spell Count", data.requirementData.spellsCount);
                EditorGUILayout.Space();
            }
            if (data.requirementData.requirements.Contains(UNLOCKING_SKILL_REQUIREMENT.tier1_count)) {
                data.requirementData.tier1Count = EditorGUILayout.IntField("Tier 1 Count", data.requirementData.tier1Count);
                EditorGUILayout.Space();
            }
            if (data.requirementData.requirements.Contains(UNLOCKING_SKILL_REQUIREMENT.tier2_count)) {
                data.requirementData.tier2Count = EditorGUILayout.IntField("Tier 2 Count", data.requirementData.tier2Count);
                EditorGUILayout.Space();
            }
            if (data.requirementData.requirements.Contains(UNLOCKING_SKILL_REQUIREMENT.tier3_count)) {
                data.requirementData.tier3Count = EditorGUILayout.IntField("Tier 3 Count", data.requirementData.tier3Count);
                EditorGUILayout.Space();
            }
        }
    }

    public void DisplaySkillListReqruiements(List<PLAYER_SKILL_TYPE> listInt, string caption) {
        var list = listInt;
        int newCount = Mathf.Max(0, EditorGUILayout.IntField(caption, list.Count));
        while (newCount < list.Count)
            list.RemoveAt(list.Count - 1);
        while (newCount > list.Count)
            list.Add(0);

        for (int i = 0; i < list.Count; i++) {
            list[i] = (PLAYER_SKILL_TYPE)EditorGUILayout.EnumPopup((PLAYER_SKILL_TYPE)list[i]);
        }
    }

    public void DisplayEnumListReqruiements(List<UNLOCKING_SKILL_REQUIREMENT> listInt, string caption) {
        var list = listInt;
        int newCount = Mathf.Max(0, EditorGUILayout.IntField(caption, list.Count));
        while (newCount < list.Count)
            list.RemoveAt(list.Count - 1);
        while (newCount > list.Count)
            list.Add(0);

        for (int i = 0; i < list.Count; i++) {
            list[i] = (UNLOCKING_SKILL_REQUIREMENT)EditorGUILayout.EnumPopup((UNLOCKING_SKILL_REQUIREMENT)list[i]);
        }
    }

    public void DisplayEnumList(List<UPGRADE_BONUS> listInt, string caption) {
        var list = listInt;
        int newCount = Mathf.Max(0, EditorGUILayout.IntField(caption, list.Count));
        while (newCount < list.Count)
            list.RemoveAt(list.Count - 1);
        while (newCount > list.Count)
            list.Add(0);

        for (int i = 0; i < list.Count; i++) {
            list[i] = (UPGRADE_BONUS)EditorGUILayout.EnumPopup((UPGRADE_BONUS)list[i]);
        }
    }

    public void DisplayIntList(List<int> listInt, string caption) {
        var list = listInt;
        int newCount = Mathf.Max(0, EditorGUILayout.IntField(caption, list.Count));
        while (newCount < list.Count)
            list.RemoveAt(list.Count - 1);
        while (newCount > list.Count)
            list.Add(0);

        for (int i = 0; i < list.Count; i++) {
            list[i] = (int)EditorGUILayout.IntField(list[i]);
        }
    }

    public void DisplayFloatList(List<float> listInt, string caption) {
        var list = listInt;
        int newCount = Mathf.Max(0, EditorGUILayout.IntField(caption, list.Count));
        while (newCount < list.Count)
            list.RemoveAt(list.Count - 1);
        while (newCount > list.Count)
            list.Add(0);

        for (int i = 0; i < list.Count; i++) {
            list[i] = (int)EditorGUILayout.FloatField(list[i]);
        }
    }
}