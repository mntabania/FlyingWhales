using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Logs;
using Object_Pools;
using UnityEngine;
using UnityEngine.Profiling;

[System.Serializable]
public class Log {
    public string persistentID;
    public string category;
    public string file;
    public string key;
    public GameDate gameDate;
    public readonly List<LOG_TAG> tags;
    public readonly List<LogFillerStruct> fillers;
    public string actionID;
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
    public bool hasValue => !string.IsNullOrEmpty(category) && !string.IsNullOrEmpty(file) && !string.IsNullOrEmpty(key);
    #endregion
        
    // public Log(GameDate date, string category, string file, string key, ActualGoapNode node = null, params LOG_TAG[] providedTags) {
    //     persistentID = UtilityScripts.Utilities.GetNewUniqueID();
    //     this.category = category;
    //     this.file = file;
    //     this.key = key;
    //     gameDate = date;
    //     _logText = LocalizationManager.Instance.GetLocalizedValue(category, file, key);
    //     rawText = string.Empty;
    //     actionID = node?.persistentID ?? string.Empty;
    //     fillers = new List<LogFillerStruct>();
    //     tags = new List<LOG_TAG>();
    //     hasBeenFinalized = false;
    //     allInvolvedObjectIDs = string.Empty;
    //     if (providedTags != null && providedTags.Length > 0) {
    //         AddTag(providedTags);
    //     } else {
    //         //always default log to misc if no tags were provided, this is to prevent logs from having no tags
    //         AddTag(LOG_TAG.Work);
    //     }
    // }
    // public Log(GameDate date, string category, string file, string key, ActualGoapNode node = null, LOG_TAG providedTag = LOG_TAG.Work) {
    //     persistentID = UtilityScripts.Utilities.GetNewUniqueID();
    //     this.category = category;
    //     this.file = file;
    //     this.key = key;
    //     gameDate = date;
    //     _logText = LocalizationManager.Instance.GetLocalizedValue(category, file, key);
    //     rawText = string.Empty;
    //     actionID = node?.persistentID ?? string.Empty;
    //     fillers = new List<LogFillerStruct>();
    //     tags = new List<LOG_TAG>();
    //     hasBeenFinalized = false;
    //     allInvolvedObjectIDs = string.Empty;
    //     AddTag(providedTag);
    // }
    // public Log(string id, GameDate date, string logText, string category, string key, string file, string involvedObjects, List<LOG_TAG> providedTags, string rawText, List<LogFillerStruct> fillers = null) {
    //     persistentID = id;
    //     this.category = category;
    //     this.file = file;
    //     this.key = key;
    //     gameDate = date;
    //     _logText = logText;
    //     actionID = string.Empty;
    //     this.fillers = fillers;
    //     tags = new List<LOG_TAG>();
    //     hasBeenFinalized = true;
    //     allInvolvedObjectIDs = involvedObjects;
    //     this.rawText = rawText;
    //     if (providedTags != null && providedTags.Count > 0) {
    //         AddTag(providedTags);
    //     } else {
    //         //always default log to misc if no tags were provided, this is to prevent logs from having no tags
    //         AddTag(LOG_TAG.Work);
    //     }
    // }
    public Log() {
        fillers = new List<LogFillerStruct>();
        tags = new List<LOG_TAG>();
        hasBeenFinalized = false;
    }

    #region Data Setting
    public void SetDate(GameDate p_date) {
        gameDate = p_date;
    }
    public void SetCategory(string s) {
        category = s;
    }
    public void SetFile(string s) {
        file = s;
    }
    public void SetKey(string s) {
        key = s;
    }
    public void DetermineInitialLogText() {
        _logText = LocalizationManager.Instance.GetLocalizedValue(category, file, key);
    }
    public void SetConnectedAction(ActualGoapNode node) {
        if (node != null) {
            actionID = node.persistentID;
        }
    }
    public void SetPersistentID(string p_id) {
        persistentID = p_id;
    }
    public void SetInvolvedObjects(string p_involved) {
        allInvolvedObjectIDs = p_involved;
    }
    public void SetRawText(string p_text) {
        rawText = p_text;
    }
    public void SetFillers(List<LogFillerStruct> p_logFillerStructs) {
        fillers.AddRange(p_logFillerStructs);
    }
    public void SetLogText(string p_logText) {
        _logText = p_logText;
    }
    public void Copy(Log p_log) {
        persistentID = p_log.persistentID;
        category = p_log.category;
        file = p_log.file;
        key = p_log.key;
        gameDate = p_log.gameDate;
        tags.Clear();
        tags.AddRange(p_log.tags);
        fillers.Clear();
        fillers.AddRange(p_log.fillers);
        actionID = p_log.actionID;
        allInvolvedObjectIDs = p_log.allInvolvedObjectIDs;
        rawText = p_log.rawText; //text without rich text tags
        hasBeenFinalized = p_log.hasBeenFinalized;
        _logText = p_log.logText;
    }
    #endregion
    
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
        ILogFiller obj = filler.GetObjectForFiller() as ILogFiller;
        if(obj != null) {
            AddInvolvedObject(obj.persistentID);
            if (obj is TileObject tileObject) {
                tileObject.OnReferencedInALog();
            }
        }
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
        if (obj == null) {
            return false;
        }
        return !string.IsNullOrEmpty(allInvolvedObjectIDs) && allInvolvedObjectIDs.Contains(obj.persistentID);
    }
    private void AddInvolvedObject(string persistentID) {
        string currentVal = allInvolvedObjectIDs;
        allInvolvedObjectIDs = $"{currentVal}|{persistentID}|";
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
    public void ResetText() {
        _logText = LocalizationManager.Instance.GetLocalizedValue(category, file, key);
    }
    public void FinalizeText() {
        _logText = UtilityScripts.Utilities.LogReplacer(_logText, fillers);
        rawText = UtilityScripts.Utilities.RemoveRichText(_logText);
        hasBeenFinalized = true;
    }
    #endregion
        
    #region Addition
    public void AddLogToDatabase(bool releaseLogAfter = false) {
#if DEBUG_PROFILER
        Profiler.BeginSample("Add Log To Database");
#endif
        DatabaseManager.Instance.mainSQLDatabase.InsertLogUsingMultiThread(this);
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif

        if (releaseLogAfter) {
            LogPool.Release(this);
        }
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
    public void AddTag(List<LOG_TAG> tags) {
        if (tags != null) {
            for (int i = 0; i < tags.Count; i++) {
                AddTag(tags[i]);
            }    
        }
    }
#endregion

#region Updates
    public bool TryUpdateLogAfterRename(Character updatedCharacter, bool force = false) {
        if (IsInvolved(updatedCharacter) || force) {
            if (fillers != null) {
                for (int i = 0; i < fillers.Count; i++) {
                    LogFillerStruct logFiller = fillers[i];
                    if (logFiller.objPersistentID == updatedCharacter.persistentID) {
                        logFiller.ForceUpdateValueBasedOnConnectedObject();
                        fillers[i] = logFiller;
                    }
                }
                ResetText();
                FinalizeText();
                return true;
            }
        }
        return false;
    }
    public void ReEvaluateWholeText() {
        // string summary = $"Re-evaluating log with id {persistentID}";
        for (int i = 0; i < fillers.Count; i++) {
            LogFillerStruct logFiller = fillers[i];
            if (logFiller.type != null && (logFiller.type == typeof(LocationStructure) || logFiller.type.IsSubclassOf(typeof(LocationStructure)))) {
                continue; //Do not update structure names because it is okay for them to be inaccurate. 
            }
            // summary = $"{summary}\nForce Updating log filler. Old value was {logFiller.value}";
            logFiller.ForceUpdateValueBasedOnConnectedObject();
            // summary = $"{summary}\nNew log filler value is {logFiller.value}";
            fillers[i] = logFiller;
        }
        ResetText();
        FinalizeText();
        // summary = $"{summary}\nFinalized text is: {logText}";
        // Debug.Log(summary);
    }
#endregion

#region Utilities
    public bool IsImportant() {
        return tags.Contains(LOG_TAG.Major);
    }
#endregion

#region Object Pools
    public void Reset() {
        fillers.Clear();
        tags.Clear();
        persistentID = string.Empty;
        category = string.Empty;
        file = string.Empty;
        key = string.Empty;
        gameDate = default;
        actionID = string.Empty;
        allInvolvedObjectIDs = string.Empty;
        rawText = string.Empty;
        hasBeenFinalized = false;
        _logText = string.Empty;
    }
#endregion
}