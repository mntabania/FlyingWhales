﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LogComponent {
    public IPointOfInterest owner { get; private set; }
    public List<Log> history { get; }
    /// <summary>
    /// History categorized by files
    /// </summary>
    private readonly Dictionary<string, List<Log>> categorizedHistory;

    private string _planCostLog;
    private const int MaxLogs = 100; 
    
    public LogComponent() {
        categorizedHistory = new Dictionary<string, List<Log>>();
        history = new List<Log>();
        ClearCostLog();
    }
    public LogComponent(SaveDataLogComponent data) {
        categorizedHistory = new Dictionary<string, List<Log>>();
        history = new List<Log>();
        ClearCostLog();
    }

    public void SetOwner(IPointOfInterest owner) {
        this.owner = owner;
    }

    #region History
    public bool AddHistory(Log log) {
        if (!history.Contains(log)) {
            log.SetDate(GameManager.Instance.Today());
            history.Add(log);
            CategorizeLog(log);
            if (history.Count > MaxLogs) {
                // history.RemoveAt(0);
                RemoveLog(history[0]);
            }
            Messenger.Broadcast(Signals.LOG_ADDED, owner);
            if (owner is Character character) {
                if (character.isLycanthrope) {
                    character.lycanData.limboForm.logComponent.AddHistory(log);
                }
            }
            return true;
        }
        return false;
    }
    private void RemoveLog(Log log) {
        if (history.Remove(log)) {
            RemoveLogFromCategorizedList(log);
            Messenger.Broadcast(Signals.LOG_REMOVED, log, owner);
        }
    }
    private void CategorizeLog(Log log) {
        if (categorizedHistory.ContainsKey(log.file) == false) {
            categorizedHistory.Add(log.file, new List<Log>());
        }
        categorizedHistory[log.file].Add(log);
    }
    private void RemoveLogFromCategorizedList(Log log) {
        if (categorizedHistory.ContainsKey(log.file)) {
            categorizedHistory[log.file].Remove(log);
        }
    }
    
    /// <summary>
    /// What should happen if another character sees this character?
    /// </summary>
    /// <param name="character">The character that saw this character.</param>
    public List<Log> GetMemories(int dayFrom, int dayTo, bool eventMemoriesOnly = false) {
        List<Log> memories = new List<Log>();
        if (eventMemoriesOnly) {
            for (int i = 0; i < history.Count; i++) {
                if (history[i].node != null) {
                    if (history[i].day >= dayFrom && history[i].day <= dayTo) {
                        memories.Add(history[i]);
                    }
                }
            }
        } else {
            for (int i = 0; i < history.Count; i++) {
                if (history[i].day >= dayFrom && history[i].day <= dayTo) {
                    memories.Add(history[i]);
                }
            }
        }
        return memories;
    }
    #endregion

    #region Notifications
    public void RegisterLog(Log addLog, GoapAction goapAction = null, bool onlyClickedCharacter = true) {
        if (!GameManager.Instance.gameHasStarted) {
            return;
        }
        addLog.AddLogToInvolvedObjects();
        // PlayerManager.Instance.player.ShowNotificationFrom(addLog, owner, onlyClickedCharacter);
    }
    #endregion

    #region Debug
    public void PrintLogIfActive(string log) {
        //if (InteriorMapManager.Instance.currentlyShowingArea == specificLocation) {//UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter == this
        Debug.Log(GameManager.Instance.TodayLogString() + log);
        //}
    }
    public void PrintLogErrorIfActive(string log) {
        //if (InteriorMapManager.Instance.currentlyShowingArea == specificLocation) {//UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter == this
        Debug.LogError(GameManager.Instance.TodayLogString() + log);
        //}
    }
    #endregion
    
    #region Goap Planning Cost Log
    public void ClearCostLog() {
        _planCostLog = string.Empty;
    }
    public void AppendCostLog(string text) {
        _planCostLog += text;
    }
    public void PrintCostLog(){
        PrintLogIfActive(_planCostLog);   
    }
    #endregion

    #region Data Getting
    public Log GetLatestLogInCategory(string category) {
        if (categorizedHistory.ContainsKey(category)) {
            return categorizedHistory[category].Last();
        }
        return null;
    }
    public List<Log> GetLogsInCategory(string category) {
        if (categorizedHistory.ContainsKey(category)) {
            return categorizedHistory[category];
        }
        return null;
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataLogComponent data) {
        for (int i = 0; i < data.history.Count; i++) {
            Log log = DatabaseManager.Instance.logDatabase.GetLogByPersistentID(data.history[i]);
            history.Add(log);
            CategorizeLog(log);
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataLogComponent : SaveData<LogComponent> {
    public List<string> history;

    #region Overrides
    public override void Save(LogComponent data) {
        history = new List<string>();
        for (int i = 0; i < data.history.Count; i++) {
            Log log = data.history[i];
            history.Add(log.persistentID);
            SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(log);
        }
    }

    public override LogComponent Load() {
        LogComponent component = new LogComponent(this);
        return component;
    }
    #endregion
}