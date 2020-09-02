using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogDatabase {
    public Dictionary<string, Log> allLogs { get; }

    public LogDatabase() {
        allLogs = new Dictionary<string, Log>();
    }

    public void AddLog(Log log) {
        if (!allLogs.ContainsKey(log.persistentID)) {
            allLogs.Add(log.persistentID, log);
        }
    }
    public Log GetLogByPersistentID(string id) {
        if (allLogs.ContainsKey(id)) {
            return allLogs[id];
        } else {
            throw new System.NullReferenceException("Trying to get a log from the database with id " + id + " but the log is not loaded");
        }
    }
}