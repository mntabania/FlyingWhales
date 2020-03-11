#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

[CustomEditor(typeof(CharacterComponent))]
public class CharacterCreator : Editor {

	CharacterComponent characterComponent;

	public override void OnInspectorGUI() {
		if(characterComponent == null){
			characterComponent = (CharacterComponent)target;
			//characterComponent.raceChoices = GetAllRaceSetups();
			characterComponent.characterClassChoices = GetAllCharacterClasses();

			//if(characterComponent.raceSettingName != string.Empty){
			//	for (int i = 0; i < characterComponent.raceChoices.Count; i++) {
			//		if(characterComponent.raceSettingName == characterComponent.raceChoices[i]){
			//			characterComponent.currRaceSelectedIndex = i;
			//			break;
			//		}
			//	}
			//}

			if(!string.IsNullOrEmpty(characterComponent.characterClassName)){
				for (int i = 0; i < characterComponent.characterClassChoices.Count; i++) {
					if(characterComponent.characterClassName == characterComponent.characterClassChoices[i]){
						characterComponent.currCharacterSelectedIndex = i;
						break;
					}
				}
			}
		}
        GUILayout.Label("Character Setup Creator ", EditorStyles.boldLabel);
		characterComponent.fileName = EditorGUILayout.TextField("File Name: ", characterComponent.fileName);
            
		//characterComponent.currRaceSelectedIndex = EditorGUILayout.Popup("Race Setup: ", characterComponent.currRaceSelectedIndex, characterComponent.raceChoices.ToArray());
		characterComponent.currCharacterSelectedIndex = EditorGUILayout.Popup("Character Class: ", characterComponent.currCharacterSelectedIndex, characterComponent.characterClassChoices.ToArray());
		characterComponent.optionalRole = (CHARACTER_ROLE)EditorGUILayout.EnumPopup("Optional Role: ", characterComponent.optionalRole);

        SerializedProperty serializedTags = serializedObject.FindProperty("tags");
        EditorGUILayout.PropertyField(serializedTags, true);
        serializedObject.ApplyModifiedProperties();

        //			SerializedProperty serializedProperty = serializedObject.FindProperty("preEquippedItems");
        //			EditorGUILayout.PropertyField(serializedProperty, true);
        //			serializedObject.ApplyModifiedProperties();

        characterComponent.itemFoldout = EditorGUILayout.Foldout(characterComponent.itemFoldout, "Pre-equipped Items");

		//Add Item NPCSettlement
		GUILayout.Space(10);
		GUILayout.BeginVertical(EditorStyles.helpBox);
		GUILayout.Label("Add Items ", EditorStyles.boldLabel);
		characterComponent.itemTypeToAdd = (ITEM_TYPE)EditorGUILayout.EnumPopup("Item Type To Add: ", characterComponent.itemTypeToAdd);
		List<string> choices = GetAllItemsOfType(characterComponent.itemTypeToAdd);
		characterComponent.currItemSelectedIndex = EditorGUILayout.Popup("Item To Add: ", characterComponent.currItemSelectedIndex, choices.ToArray());
		GUI.enabled = choices.Count > 0;
		if (GUILayout.Button("Add Item")) {
			characterComponent.AddItem(choices[characterComponent.currItemSelectedIndex]);
		}
		GUI.enabled = true;
		GUILayout.EndHorizontal();



		//characterComponent.raceSettingName = characterComponent.raceChoices[characterComponent.currRaceSelectedIndex];
		characterComponent.characterClassName = characterComponent.characterClassChoices[characterComponent.currCharacterSelectedIndex];

		if (GUILayout.Button("Save Character")) {
			SaveCharacter(ConstructCharacterSetup());
		}
    }

    #region Character Classes
    private List<string> GetAllCharacterClasses() {
        List<string> allCharacterClasses = new List<string>();
        string path = $"{UtilityScripts.Utilities.dataPath}CharacterClasses/";
        foreach (string file in Directory.GetFiles(path, "*.json")) {
            allCharacterClasses.Add(Path.GetFileNameWithoutExtension(file));
        }
        return allCharacterClasses;
    }
    #endregion

    #region Race Setups
    private List<string> GetAllRaceSetups() {
        List<string> allRaceSetups = new List<string>();
        string path = $"{UtilityScripts.Utilities.dataPath}RaceSettings/";
        foreach (string file in Directory.GetFiles(path, "*.json")) {
            allRaceSetups.Add(Path.GetFileNameWithoutExtension(file));
        }
        return allRaceSetups;
    }
    #endregion

    private void SaveCharacter(CharacterSetup characterSetup) {
		string path = $"{UtilityScripts.Utilities.dataPath}CharacterSetups/{characterComponent.fileName}.json";
        if (File.Exists(path)) {
			if (EditorUtility.DisplayDialog("Overwrite Character",
				$"{characterComponent.fileName} already exists. Replace with this character?", "Yes", "No")) {
                File.Delete(path);
                SaveCharacterJson(path, characterSetup);
            }
        } else {
            SaveCharacterJson(path, characterSetup);
        }
    }
	private CharacterSetup ConstructCharacterSetup() {
        CharacterSetup newCharacter = new CharacterSetup();
        newCharacter.fileName = characterComponent.fileName;
		//newCharacter.raceSettingName = characterComponent.raceSettingName;
		newCharacter.characterClassName = characterComponent.characterClassName;
		newCharacter.optionalRole = characterComponent.optionalRole;
        newCharacter.tags = characterComponent.tags;

        return newCharacter;
    }
    private void SaveCharacterJson(string path, CharacterSetup character) {
        string jsonString = JsonUtility.ToJson(character);

        System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
        writer.WriteLine(jsonString);
        writer.Close();

        //Re-import the file to update the reference in the editor
        UnityEditor.AssetDatabase.ImportAsset(path);
        Debug.Log($"Successfully saved character data at {path}");
    }

	private List<string> GetAllItemsOfType(ITEM_TYPE itemType) {
		List<string> allItemsOfType = new List<string>();
		string path = $"{UtilityScripts.Utilities.dataPath}Items/{itemType}/";
		foreach (string file in System.IO.Directory.GetFiles(path, "*.json")) {
			allItemsOfType.Add(System.IO.Path.GetFileNameWithoutExtension(file));
		}
		return allItemsOfType;
	}
}
#endif