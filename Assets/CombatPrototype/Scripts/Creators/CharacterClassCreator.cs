#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(ClassComponent))]
public class CharacterClassCreator : Editor {

	ClassComponent currCharacterClass;

    public override void OnInspectorGUI() {
		if(currCharacterClass == null){
			currCharacterClass = (ClassComponent)target;
		}
        GUILayout.Label("Class Creator ", EditorStyles.boldLabel);
        currCharacterClass.className = EditorGUILayout.TextField("Class Name: ", currCharacterClass.className);
        currCharacterClass.baseAttackPower = EditorGUILayout.FloatField("Base Attack Power: ", currCharacterClass.baseAttackPower);
        currCharacterClass.attackPowerPerLevel = EditorGUILayout.FloatField("Attack Power Per Level: ", currCharacterClass.attackPowerPerLevel);
        currCharacterClass.baseSpeed = EditorGUILayout.FloatField("Base Speed: ", currCharacterClass.baseSpeed);
        currCharacterClass.speedPerLevel = EditorGUILayout.FloatField("Speed Per Level: ", currCharacterClass.speedPerLevel);
        currCharacterClass.baseHP = EditorGUILayout.IntField("Base HP: ", currCharacterClass.baseHP);
        currCharacterClass.hpPerLevel = EditorGUILayout.IntField("HP Per Level: ", currCharacterClass.hpPerLevel);
        currCharacterClass.baseSP = EditorGUILayout.IntField("Base SP: ", currCharacterClass.baseSP);
        currCharacterClass.spPerLevel = EditorGUILayout.IntField("SP Per Level: ", currCharacterClass.spPerLevel);
        currCharacterClass.workActionType = (ACTION_TYPE)EditorGUILayout.EnumPopup("Work Action: ", currCharacterClass.workActionType);

        //currCharacterClass.dodgeRate = EditorGUILayout.IntField("Dodge Rate: ", currCharacterClass.dodgeRate);
        //currCharacterClass.parryRate = EditorGUILayout.IntField("Parry Rate: ", currCharacterClass.parryRate);
        //currCharacterClass.blockRate = EditorGUILayout.IntField("Block Rate: ", currCharacterClass.blockRate);

        SerializedProperty allowedWeaponType = serializedObject.FindProperty("allowedWeaponTypes");
		EditorGUILayout.PropertyField(allowedWeaponType, true);
		serializedObject.ApplyModifiedProperties ();

        SerializedProperty harvestResource = serializedObject.FindProperty("harvestResources");
        EditorGUILayout.PropertyField(harvestResource, true);
        serializedObject.ApplyModifiedProperties();

        SerializedProperty skillsPerLevel = serializedObject.FindProperty("skillsPerLevel");
        EditorGUILayout.PropertyField(skillsPerLevel, true);
        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Create Character Class")) {
            SaveCharacterClass();
        }
    }

    #region Saving
    private void SaveCharacterClass() {
        if (string.IsNullOrEmpty(currCharacterClass.className)) {
            EditorUtility.DisplayDialog("Error", "Please specify a Class Name", "OK");
            return;
        }
        string path = $"{UtilityScripts.Utilities.dataPath}CharacterClasses/{currCharacterClass.className}.json";
        if (UtilityScripts.Utilities.DoesFileExist(path)) {
            if (EditorUtility.DisplayDialog("Overwrite Class",
                $"A class with name {currCharacterClass.className} already exists. Replace with this class?", "Yes", "No")) {
                File.Delete(path);
                SaveCharacterClassJson(currCharacterClass, path);
            }
        } else {
            SaveCharacterClassJson(currCharacterClass, path);
        }
    }
    private void SaveCharacterClassJson(ClassComponent classComponent, string path) {
        CharacterClass characterClass = new CharacterClass();
        characterClass.SetData(classComponent);
        //if (classComponent.skillsPerLevelNames == null) {
        //    classComponent.skillsPerLevelNames = new List<StringListWrapper>();
        //} else {
        //    classComponent.skillsPerLevelNames.Clear();
        //}
        //for (int i = 0; i < classComponent.skillsPerLevel.Count; i++) {
        //    StringListWrapper skillNames = new StringListWrapper();
        //    for (int j = 0; j < classComponent.skillsPerLevel[i].list.Count; j++) {
        //        skillNames.list.Add(classComponent.skillsPerLevel[i].list[j].name);
        //    }
        //    classComponent.skillsPerLevelNames.Add(skillNames);
        //}
        string jsonString = JsonUtility.ToJson(characterClass);
        System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
        writer.WriteLine(jsonString);
        writer.Close();
        UnityEditor.AssetDatabase.ImportAsset(path);
        Debug.Log($"Successfully saved class {characterClass.className} at {path}");
    }
    #endregion

    private void ResetValues() {
        currCharacterClass = null;
    }
}
#endif