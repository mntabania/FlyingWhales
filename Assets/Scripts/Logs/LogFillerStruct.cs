﻿using Inner_Maps.Location_Structures;
using Logs;
using UnityEngine.Assertions;

[System.Serializable]
public struct LogFillerStruct {
    public System.Type type;
    public string value;
    public string objPersistentID; //the persistent ID of the object referenced by this filler.
    public LOG_IDENTIFIER identifier;

    public LogFillerStruct(ILogFiller obj, string value, LOG_IDENTIFIER identifier) {
        if (obj != null) {
            type = obj.GetType();
            objPersistentID = obj.persistentID;
        }else {
            type = null;
            objPersistentID = string.Empty;
        }
        this.value = value;
        this.identifier = identifier;
    }
        
        
    public object GetObjectForFiller() {
        if (type != null && !string.IsNullOrEmpty(objPersistentID)) {
            return DatabaseManager.Instance.GetObjectFromDatabase(type, objPersistentID);
        }
        return null;
    }
    public string GetLinkText() {
        Assert.IsNotNull(type, $"Filler {this.ToString()} is trying to create a link tag, but it shouldn't since it doesn't have an object attached to it.");
        string typeString = type.ToString();
        if (type.IsSubclassOf(typeof(TileObject))) {
            typeString = "TileObject"; //Need this because if type of tile object and name is the same, when hovered the underline code will underline this link text instead. (i.e. Table)
        }
        return $"{typeString}|{objPersistentID}";
    }
    /// <summary>
    /// This is the format of this filler when it is placed inside the database.
    /// </summary>
    /// <returns>Formatted string for placement in SQL database.</returns>
    public string GetSQLText() {
        //Need to replace single quotes in log message to two single quotes to prevent SQL command errors
        //Reference: https://stackoverflow.com/questions/603572/escape-single-quote-character-for-use-in-an-sqlite-query
        if (type == null) {
            return value.Replace("'", "''");
        } else {
            return $"{type}|{value.Replace("'", "''")}|{objPersistentID}";    
        }
    }
    public override string ToString() {
        return $"{type?.ToString() ?? "NoType"} - {value} - {objPersistentID} - {identifier.ToString()}";
    }
}