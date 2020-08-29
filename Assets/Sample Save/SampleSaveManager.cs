using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class SampleSaveManager : MonoBehaviour
{
    SampleSave loadedSave;

    public void Save() {
        SampleSave save = new SampleSave();
        save.Save();
        SaveData($"{UtilityScripts.Utilities.gameSavePath}SAVED_CURRENT_PROGRESS.sav", save);
    }
    public void Load() {
        loadedSave = LoadData<SampleSave>($"{UtilityScripts.Utilities.gameSavePath}SAVED_CURRENT_PROGRESS.sav");

        string text = "CHARACTERS";
        for (int i = 0; i < loadedSave.characters.Count; i++) {
            SampleSaveCharacter character = loadedSave.characters[i];
            text += character.GetText();
            foreach (KeyValuePair<int, SampleSaveFaction> item in loadedSave.factions) {
                text += "\nFaction 1 == Loaded Faction " + item.Key + (character.faction == item.Value);
            }
            foreach (KeyValuePair<int, SampleSaveFaction> item in loadedSave.factions) {
                text += "\nFaction 2 == Loaded Faction " + item.Key + (character.faction2 == item.Value);
            }
        }

        text += "\n\nFACTIONS";
        foreach (KeyValuePair<int, SampleSaveFaction> item in loadedSave.factions) {
            text += item.Value.GetText();
        }
        Debug.Log(text);
    }

    public void SaveData<T>(string identifier, T obj) {
        if (string.IsNullOrEmpty(identifier)) {
            throw new System.ArgumentNullException("identifier");
        }
        string filePath = "";
        if (IsFilePath(identifier)) {
            filePath = identifier;
        } else {
            throw new System.Exception("identifier is not a file path!");
        }
        if (obj == null) {
            throw new System.Exception("Object to be saved is null!");
        }
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        string json = JsonConvert.SerializeObject(obj, Formatting.Indented,
            new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.All, ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor, ReferenceLoopHandling = ReferenceLoopHandling.Serialize });

        File.WriteAllText(filePath, json);

    }

    public T LoadData<T>(string identifier) {
        if (string.IsNullOrEmpty(identifier)) {
            throw new System.ArgumentNullException("identifier");
        }
        string filePath = "";
        if (IsFilePath(identifier)) {
            filePath = identifier;
        } else {
            throw new System.Exception("identifier is not a file path!");
        }
        string data = File.ReadAllText(filePath);

        T convertedObj = JsonConvert.DeserializeObject<T> (data, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.All, ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor, ReferenceLoopHandling = ReferenceLoopHandling.Serialize });

        return convertedObj;
    }

    public bool IsFilePath(string str) {
        bool result = false;
        if (Path.IsPathRooted(str)) {
            try {
                Path.GetFullPath(str);
                result = true;
            } catch (System.Exception) {
                result = false;
            }
        }
        return result;
    }
}
