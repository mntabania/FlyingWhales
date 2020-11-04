#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(MonsterComponent))]
public class MonsterCreator : Editor {

    MonsterComponent monsterComponent;

    public override void OnInspectorGUI() {
        if (monsterComponent == null) {
            monsterComponent = (MonsterComponent) target;
        }
        GUILayout.Label("Monster Creator ", EditorStyles.boldLabel);
        monsterComponent.monsterName = EditorGUILayout.TextField("Name: ", monsterComponent.monsterName);
        monsterComponent.type = (MONSTER_TYPE) EditorGUILayout.EnumPopup("Type: ", monsterComponent.type);
        monsterComponent.category = (MONSTER_CATEGORY) EditorGUILayout.EnumPopup("Category: ", monsterComponent.category);
        monsterComponent.level = EditorGUILayout.IntField("Level: ", monsterComponent.level);
        monsterComponent.experienceDrop = EditorGUILayout.IntField("Exp Drop: ", monsterComponent.experienceDrop);
        monsterComponent.maxHP = EditorGUILayout.IntField("Max HP: ", monsterComponent.maxHP);
        monsterComponent.maxSP = EditorGUILayout.IntField("Max SP: ", monsterComponent.maxSP);
        monsterComponent.attackPower = EditorGUILayout.IntField("Attack Power: ", monsterComponent.attackPower);
        monsterComponent.speed = EditorGUILayout.IntField("Speed: ", monsterComponent.speed);
        monsterComponent.pDef = EditorGUILayout.IntField("PDef: ", monsterComponent.pDef);
        monsterComponent.mDef = EditorGUILayout.IntField("MDef: ", monsterComponent.mDef);
        monsterComponent.dodgeChance = EditorGUILayout.FloatField("Dodge Chance: ", monsterComponent.dodgeChance);
        monsterComponent.hitChance = EditorGUILayout.FloatField("Hit Chance: ", monsterComponent.hitChance);
        monsterComponent.critChance = EditorGUILayout.FloatField("Crit Chance: ", monsterComponent.critChance);

        SerializedProperty serializedProperty = serializedObject.FindProperty("skills");
        EditorGUILayout.PropertyField(serializedProperty, true);
        serializedObject.ApplyModifiedProperties();

        SerializedProperty elementChanceWeakness = serializedObject.FindProperty("elementChanceWeaknesses");
        EditorGUILayout.PropertyField(elementChanceWeakness, true);
        serializedObject.ApplyModifiedProperties();

        SerializedProperty elementChanceResistance = serializedObject.FindProperty("elementChanceResistances");
        EditorGUILayout.PropertyField(elementChanceResistance, true);
        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Create Monster")) {
            SaveMonster();
        }
    }

    #region Saving
    private void SaveMonster() {
        if (string.IsNullOrEmpty(monsterComponent.monsterName)) {
            EditorUtility.DisplayDialog("Error", "Please specify a Monster Name", "OK");
            return;
        }
        string path = Utilities.dataPath + "Monsters/" + monsterComponent.monsterName + ".json";
        if (Utilities.DoesFileExist(path)) {
            if (EditorUtility.DisplayDialog("Overwrite Monster", "A monster with name " + monsterComponent.monsterName + " already exists. Replace with this monster?", "Yes", "No")) {
                File.Delete(path);
                SaveMonsterJson(path);
            }
        } else {
            SaveMonsterJson(path);
        }
    }
    private void SaveMonsterJson(string path) {
        if (monsterComponent.skillNames == null) {
            monsterComponent.skillNames = new List<string>();
        } else {
            monsterComponent.skillNames.Clear();
        }
        for (int i = 0; i < monsterComponent.skills.Count; i++) {
            monsterComponent.skillNames.Add(monsterComponent.skills[i].name);
        }
        string jsonString = JsonUtility.ToJson(monsterComponent);
        System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
        writer.WriteLine(jsonString);
        writer.Close();
        UnityEditor.AssetDatabase.ImportAsset(path);
        Debug.Log("Successfully saved monster " + monsterComponent.monsterName + " at " + path);
    }
    #endregion

    private void ResetValues() {
        monsterComponent = null;
    }
}
#endif