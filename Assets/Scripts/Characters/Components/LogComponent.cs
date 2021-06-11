using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LogComponent {
    //public IPointOfInterest owner { get; private set; }
    // public List<Log> history { get; }
    // /// <summary>
    // /// History categorized by files
    // /// </summary>
    // private readonly Dictionary<string, List<Log>> categorizedHistory;

    private string _planCostLog;
    // private const int MaxLogs = 100; 
    
    public LogComponent() {
        // categorizedHistory = new Dictionary<string, List<Log>>();
        // history = new List<Log>();
        ClearCostLog();
    }
    public LogComponent(SaveDataLogComponent data) {
        // categorizedHistory = new Dictionary<string, List<Log>>();
        // history = new List<Log>();
        ClearCostLog();
    }

    //public void SetOwner(IPointOfInterest owner) {
    //    this.owner = owner;
    //}

    // #region History
    // public bool AddHistory(Log log) {
    //     if (!history.Contains(log)) {
    //         log.SetDate(GameManager.Instance.Today());
    //         history.Add(log);
    //         CategorizeLog(log);
    //         if (history.Count > MaxLogs) {
    //             // history.RemoveAt(0);
    //             RemoveLog(history[0]);
    //         }
    //         Messenger.Broadcast(Signals.LOG_ADDED, owner);
    //         if (owner is Character character) {
    //             if (character.isLycanthrope) {
    //                 character.lycanData.limboForm.logComponent.AddHistory(log);
    //             }
    //         }
    //         return true;
    //     }
    //     return false;
    // }
    // private void RemoveLog(Log log) {
    //     if (history.Remove(log)) {
    //         RemoveLogFromCategorizedList(log);
    //         Messenger.Broadcast(Signals.LOG_REMOVED, log, owner);
    //     }
    // }
    // private void CategorizeLog(Log log) {
    //     if (categorizedHistory.ContainsKey(log.file) == false) {
    //         categorizedHistory.Add(log.file, new List<Log>());
    //     }
    //     categorizedHistory[log.file].Add(log);
    // }
    // private void RemoveLogFromCategorizedList(Log log) {
    //     if (categorizedHistory.ContainsKey(log.file)) {
    //         categorizedHistory[log.file].Remove(log);
    //     }
    // }
    // #endregion

    #region Notifications
    public void RegisterLog(Log addLog, bool releaseAfter = false) {
        if (!GameManager.Instance.gameHasStarted) {
            return;
        }
        addLog.AddLogToDatabase(releaseAfter);
    }
    #endregion

    #region Debug
    public void PrintLogIfActive(string log) {
#if DEBUG_LOG
        Debug.Log(GameManager.Instance.TodayLogString() + log);
#endif
    }
    public void PrintLogErrorIfActive(string log) {
#if DEBUG_LOG
        Debug.LogError(GameManager.Instance.TodayLogString() + log);
#endif
    }
    #endregion

    #region Goap Planning Cost Log
    public void ClearCostLog() {
        _planCostLog = string.Empty;
    }
    public void AppendCostLog(string text) {
#if DEBUG_LOG
        _planCostLog += text;
#endif
    }
    public void PrintCostLog(){
        PrintLogIfActive(_planCostLog);   
    }
#endregion

    // #region Loading
    // public void LoadReferences(SaveDataLogComponent data) {
    //     for (int i = 0; i < data.history.Count; i++) {
    //         Log log = DatabaseManager.Instance.logDatabase.GetLogByPersistentID(data.history[i]);
    //         history.Add(log);
    //         CategorizeLog(log);
    //     }
    // }
    // #endregion
}

[System.Serializable]
public class SaveDataLogComponent : SaveData<LogComponent> {
    // public List<string> history;

#region Overrides
    public override void Save(LogComponent data) {
        // history = new List<string>();
        // for (int i = 0; i < data.history.Count; i++) {
        //     Log log = data.history[i];
        //     history.Add(log.persistentID);
        //     if (log.node != null) {
        //         SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(log.node);    
        //     }
        //     SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(log);
        // }
    }

    public override LogComponent Load() {
        LogComponent component = new LogComponent(this);
        return component;
    }
#endregion
}