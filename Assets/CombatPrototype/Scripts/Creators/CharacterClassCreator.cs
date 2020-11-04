#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ECS {
	[CustomEditor(typeof(ClassComponent))]
    public class CharacterClassCreator : Editor {

		ClassComponent currCharacterClass;

        public override void OnInspectorGUI() {
			if(currCharacterClass == null){
				currCharacterClass = (ClassComponent)target;
			}
            GUILayout.Label("Class Creator ", EditorStyles.boldLabel);
            currCharacterClass.className = EditorGUILayout.TextField("Class Name: ", currCharacterClass.className);
            currCharacterClass.strWeightAllocation = EditorGUILayout.FloatField("Strength Weight Allocation: ", currCharacterClass.strWeightAllocation);
            currCharacterClass.intWeightAllocation = EditorGUILayout.FloatField("Intelligence Weight Allocation: ", currCharacterClass.intWeightAllocation);
            currCharacterClass.agiWeightAllocation = EditorGUILayout.FloatField("Agility Weight Allocation: ", currCharacterClass.agiWeightAllocation);
            currCharacterClass.vitWeightAllocation = EditorGUILayout.FloatField("Vitality Weight Allocation: ", currCharacterClass.vitWeightAllocation);
            currCharacterClass.hpModifier = EditorGUILayout.FloatField("HP Modifier: ", currCharacterClass.hpModifier);
            currCharacterClass.spModifier = EditorGUILayout.FloatField("SP Modifier: ", currCharacterClass.spModifier);
            //currCharacterClass.dodgeRate = EditorGUILayout.IntField("Dodge Rate: ", currCharacterClass.dodgeRate);
            //currCharacterClass.parryRate = EditorGUILayout.IntField("Parry Rate: ", currCharacterClass.parryRate);
            //currCharacterClass.blockRate = EditorGUILayout.IntField("Block Rate: ", currCharacterClass.blockRate);

            SerializedProperty allowedWeaponType = serializedObject.FindProperty("allowedWeaponTypes");
			EditorGUILayout.PropertyField(allowedWeaponType, true);
			serializedObject.ApplyModifiedProperties ();

            SerializedProperty skillsPerLevel = serializedObject.FindProperty("skillsPerLevel");
            EditorGUILayout.PropertyField(skillsPerLevel, true);
            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Create ECS.Character Class")) {
                SaveCharacterClass();
            }
        }

        #region Saving
        private void SaveCharacterClass() {
            if (string.IsNullOrEmpty(currCharacterClass.className)) {
                EditorUtility.DisplayDialog("Error", "Please specify a Class Name", "OK");
                return;
            }
            string path = Utilities.dataPath + "CharacterClasses/" + currCharacterClass.className + ".json";
            if (Utilities.DoesFileExist(path)) {
                if (EditorUtility.DisplayDialog("Overwrite Class", "A class with name " + currCharacterClass.className + " already exists. Replace with this class?", "Yes", "No")) {
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
            Debug.Log("Successfully saved class " + characterClass.className + " at " + path);
        }
        #endregion

        private void ResetValues() {
            currCharacterClass = null;
        }
    }
}
#endif