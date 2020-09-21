using System.Collections.Generic;
using Logs;
using UnityEngine;

[System.Serializable]
public struct Log {
    public readonly string persistentID;
    public readonly string category;
    public readonly string file;
    public readonly string key;
    public readonly GameDate gameDate;
    public readonly List<LOG_TAG> tags;
    public readonly List<LogFillerStruct> fillers;
    public readonly string actionID;
    public readonly bool hasValue;
    public string allInvolvedObjectIDs;
    public string rawText; //text without rich text tags
    [SerializeField] private bool hasBeenFinalized;
    [SerializeField] private string _logText;
    
    #region getters
    public string logText {
        get {
            if (!hasBeenFinalized) {
                FinalizeText();
            }
            return _logText;
        }
    }
    public string unReplacedText => LocalizationManager.Instance.GetLocalizedValue(category, file, key);
    #endregion
        
    public Log(GameDate date, string category, string file, string key, ActualGoapNode node = null, params LOG_TAG[] providedTags) {
        persistentID = UtilityScripts.Utilities.GetNewUniqueID();
        this.category = category;
        this.file = file;
        this.key = key;
        gameDate = date;
        _logText = LocalizationManager.Instance.GetLocalizedValue(category, file, key);
        rawText = string.Empty;
        actionID = node?.persistentID ?? string.Empty;
        fillers = new List<LogFillerStruct>();
        tags = providedTags != null && providedTags.Length > 0 ? new List<LOG_TAG>(providedTags) : new List<LOG_TAG>() { LOG_TAG.Misc }; //always default log to misc if no tags were provided, this is to prevent logs from having no tags
        hasValue = true;
        hasBeenFinalized = false;
        allInvolvedObjectIDs = string.Empty;
    }
    public Log(string id, GameDate date, string logText, string category, string key, string file, string involvedObjects, List<LOG_TAG> providedTags, string rawText) {
        persistentID = id;
        this.category = category;
        this.file = key;
        this.key = file;
        gameDate = date;
        _logText = logText;
        actionID = string.Empty;
        fillers = null;
        tags = providedTags != null && providedTags.Count > 0 ? new List<LOG_TAG>(providedTags) : new List<LOG_TAG>() { LOG_TAG.Misc }; //always default log to misc if no tags were provided, this is to prevent logs from having no tags
        hasValue = true;
        hasBeenFinalized = true;
        allInvolvedObjectIDs = involvedObjects;
        this.rawText = rawText;
    }

    #region Fillers
    internal void AddToFillers(ILogFiller obj, string value, LOG_IDENTIFIER identifier, bool replaceExisting = true){
        if (replaceExisting && HasFillerForIdentifier(identifier)) {
            fillers.Remove(GetFillerForIdentifier(identifier));
        }
        if (obj != null) {
            AddInvolvedObject(obj.persistentID);
            if (obj is TileObject tileObject) {
                tileObject.OnReferencedInALog();
            }
        }
        
        fillers.Add (new LogFillerStruct(obj, value, identifier));
    }
    internal void AddToFillers(LogFillerStruct filler) {
        fillers.Add (filler);
    }
    internal void AddToFillers(List<LogFillerStruct> fillers) {
        for (int i = 0; i < fillers.Count; i++) {
            LogFillerStruct filler = fillers[i];
            AddToFillers(filler);
        }
    }
    private bool HasFillerForIdentifier(LOG_IDENTIFIER identifier) {
        for (int i = 0; i < fillers.Count; i++) {
            LogFillerStruct currFiller = fillers[i];
            if (currFiller.identifier == identifier) {
                return true;
            }
        }
        return false;
    }
    private LogFillerStruct GetFillerForIdentifier(LOG_IDENTIFIER identifier) {
        for (int i = 0; i < fillers.Count; i++) {
            LogFillerStruct currFiller = fillers[i];
            if (currFiller.identifier == identifier) {
                return currFiller;
            }
        }
        return default(LogFillerStruct);
    }
    public bool DoesLogUseIdentifier(LOG_IDENTIFIER logIdentifier) {
        if (UtilityScripts.Utilities.logIdentifierStrings.ContainsKey(logIdentifier)) {
            string logString = UtilityScripts.Utilities.logIdentifierStrings[logIdentifier];
            if (unReplacedText.Contains(logString)) {
                return true;
            }
        }
        return false;
    }
    public bool HasFillerThatMeetsRequirement(System.Func<object, bool> requirement) {
         for (int i = 0; i < fillers.Count; i++) {
             LogFillerStruct filler = fillers[i];
             object obj = filler.GetObjectForFiller();
             if (obj != null && requirement.Invoke(obj)) {
                 return true;
             }
         }
         return false;
    }
    public bool IsInvolved(ILogFiller obj) {
        return allInvolvedObjectIDs.Contains(obj.persistentID);
    }
    public void AddInvolvedObject(string persistentID) {
        allInvolvedObjectIDs = $"{allInvolvedObjectIDs}|{persistentID}|";
    }
    /// <summary>
    /// Manually add an involved object. This is usually needed
    /// </summary>
    /// <param name="persistentID"></param>
    public void AddInvolvedObjectManual(string persistentID) {
        AddInvolvedObject(persistentID);
        DatabaseManager.Instance.mainSQLDatabase.UpdateInvolvedObjects(this);
    }
    #endregion

    #region Text
    public void FinalizeText() {
        _logText = UtilityScripts.Utilities.LogReplacer(_logText, fillers);
        rawText = UtilityScripts.Utilities.RemoveRichText(_logText);
        hasBeenFinalized = true;
    }
    #endregion
        
    #region Addition
    public void AddLogToDatabase() {
        DatabaseManager.Instance.mainSQLDatabase.InsertLog(this);
    }
    #endregion

    #region Tags
    public void AddTag(LOG_TAG tag) {
        if (!tags.Contains(tag)) {
            tags.Add(tag);    
        }
    }
    public void AddTag(LOG_TAG[] tags) {
        if (tags != null) {
            for (int i = 0; i < tags.Length; i++) {
                AddTag(tags[i]);
            }    
        }
    }
    #endregion
}