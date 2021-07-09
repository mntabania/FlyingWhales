using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Newtonsoft.Json;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;

public static class SaveUtilities {


    public static List<string> compatibleSaveFileVersions = new List<string>() {
        "0.5", "0.5.01", "0.5.02"
    };
    
    #region Character States
    /// <summary>
    /// Convenience function to create a new Save Data instance for a character state.
    /// NOTE: This does not place the actual values of the state, it only creates an instance.
    /// </summary>
    /// <param name="characterState">The state to create the save data for.</param>
    /// <returns>New save data instance.</returns>
    public static SaveDataCharacterState CreateCharacterStateSaveDataInstance(CharacterState characterState) {
        if (characterState.characterState.HasUniqueSaveData()) {
            string suffix = typeof(SaveDataCharacterState).ToString(); //this is for convenience of renaming the class. nothing more
            string wholeTypeName = characterState.GetType().ToString() + suffix;
            System.Type systemType = System.Type.GetType(wholeTypeName);
            return System.Activator.CreateInstance(systemType) as SaveDataCharacterState;
        } else {
            return new SaveDataCharacterState();
        }
    }
    #endregion

    public static List<string> ConvertSavableListToIDs<T>(List<T> savables) where T : ISavable{
        List<string> ids = new List<string>();
        if(savables != null && savables.Count > 0) {
            for (int i = 0; i < savables.Count; i++) {
                ids.Add(savables[i].persistentID);
            }
        }
        return ids;
    } 
    public static List<Character> ConvertIDListToCharacters(List<string> ids) {
        List<Character> objects = new List<Character>();
        for (int i = 0; i < ids.Count; i++) {
            string pid = ids[i];
            Character character = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(pid);
            if (character != null) {
                objects.Add(character);    
            }
        }
        return objects;
    }
    public static List<Summon> ConvertIDListToMonsters(List<string> ids) {
        List<Summon> objects = new List<Summon>();
        for (int i = 0; i < ids.Count; i++) {
            string pid = ids[i];
            Character character = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(pid);
            if (character is Summon summon) {
                objects.Add(summon);    
            }
        }
        return objects;
    }
    public static List<TileObject> ConvertIDListToTileObjects(List<string> ids) {
        List<TileObject> objects = new List<TileObject>();
        for (int i = 0; i < ids.Count; i++) {
            string pid = ids[i];
            TileObject tileObject = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(pid);
            objects.Add(tileObject);
        }
        return objects;
    }
    public static List<LocationGridTile> ConvertIDListToLocationGridTiles(List<string> ids) {
        List<LocationGridTile> objects = new List<LocationGridTile>();
        for (int i = 0; i < ids.Count; i++) {
            string pid = ids[i];
            LocationGridTile tileObject = DatabaseManager.Instance.locationGridTileDatabase.GetTileByPersistentID(pid);
            objects.Add(tileObject);
        }
        return objects;
    }
    public static List<LocationStructure> ConvertIDListToStructures(List<string> ids) {
        List<LocationStructure> objects = new List<LocationStructure>();
        for (int i = 0; i < ids.Count; i++) {
            string pid = ids[i];
            LocationStructure tileObject = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(pid);
            objects.Add(tileObject);
        }
        return objects;
    }
    public static List<Faction> ConvertIDListToFactions(List<string> ids) {
        List<Faction> objects = new List<Faction>();
        for (int i = 0; i < ids.Count; i++) {
            string pid = ids[i];
            Faction faction = DatabaseManager.Instance.factionDatabase.GetFactionBasedOnPersistentID(pid);
            objects.Add(faction);
        }
        return objects;
    }

    public static SaveDataJobNode createSaveDataJobNode(JobNode jobNode) {
        return new SaveDataSingleJobNode();
    }

    #region Tile Objects
    public static SaveDataTileObject CreateNewSaveDataForTileObject(string tileObjectTypeString) {
        var typeName = $"SaveData{tileObjectTypeString}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        System.Type type = System.Type.GetType(typeName);
        if (type != null) {
            SaveDataTileObject obj = System.Activator.CreateInstance(type) as SaveDataTileObject;
            return obj;
        }
        return new SaveDataTileObject(); //if no special save data for tile object was found, then just use the generic one
    }
    public static SaveDataTileObject CreateNewSaveDataForArtifact(string tileObjectTypeString) {
        var typeName = $"SaveData{tileObjectTypeString}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        System.Type type = System.Type.GetType(typeName);
        if (type != null) {
            SaveDataTileObject obj = System.Activator.CreateInstance(type) as SaveDataTileObject;
            return obj;
        }
        return new SaveDataArtifact(); //if no special save data for tile object was found, then just use the generic one
    }
    #endregion
    
    public static SaveDataStructureRoom CreateSaveDataForRoom(StructureRoom structureRoom) {
        if (structureRoom is PrisonCell) {
            return new SaveDataPrisonCell();
        }
        return new SaveDataStructureRoom();
    }

    #region Save File
    public static string GetGameVersionOfSaveFile(string json) {
        var reader = new JsonTextReader(new StringReader(json));
        string currentProperty = string.Empty;
        while (reader.Read()) {
            if (reader.Value != null) {
                if (reader.TokenType == JsonToken.PropertyName) {
                    currentProperty = reader.Value.ToString();
                }
                if (currentProperty == "gameVersion") {
                    if (reader.TokenType == JsonToken.String) {
                        return reader.Value.ToString();
                    }
                }
            }
        }
        return string.Empty;
    }
    public static bool IsSaveFileValid(string path) {
        string json = string.Empty;
        using (ZipArchive zip = ZipFile.Open(path, ZipArchiveMode.Read)) {
            foreach (ZipArchiveEntry entry in zip.Entries) {
                if (entry.Name == "mainSave.sav") {
                    using (StreamReader reader = new StreamReader(entry.Open())) {
                        json = reader.ReadToEnd();
                    }
                    break;
                }
            }
        }
        if (!string.IsNullOrEmpty(json)) {
            string saveFileVersion = GetGameVersionOfSaveFile(json);
            return saveFileVersion == Application.version || compatibleSaveFileVersions.Contains(saveFileVersion);
        }
        return false;
    }
    #endregion
}
