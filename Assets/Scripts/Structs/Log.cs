using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//Log and Memory are the same now so assume that this class will have data that the Memory uses
public class Log : ISavable {
    public string persistentID { get; private set; }
    public int id;

	public MONTH month;
	public int day;
	public int year;
    public int tick;

    public string category;
	public string file;
	public string key;

    public LOG_TYPE logType;

    public string message;

	public List<LogFiller> fillers;

    //private bool lockFillers;

    public string logCallStack;



    //When this log is processed through the LogReplacer for the first time, the resulting text will be stored in this so that every time the text of this log is needed,
    //it will not go through the LogReplacer processing again, which saves cpu power
    public string logText { get; private set; }

    //Memory data
    private ActualGoapNode _node;

    #region getters
    public GameDate date => new GameDate((int) month, day, year, tick);
    public ActualGoapNode node => GetNodeAssociatedWithThisLog();
    public OBJECT_TYPE objectType => OBJECT_TYPE.Log;
    public System.Type serializedData => typeof(SaveDataLog);
    #endregion

    public Log(GameDate date, string category, string file, string key, ActualGoapNode node = null) {
        persistentID = UtilityScripts.Utilities.GetNewUniqueID();
        this.id = UtilityScripts.Utilities.SetID(this);
        this.month = (MONTH)date.month;
        this.day = date.day;
        this.year = date.year;
        this.tick = date.tick;
        this.category = category;
        this.file = file;
        this.key = key;
        this._node = node;
        this.fillers = new List<LogFiller>();
        //this.lockFillers = false;
        logText = string.Empty;
        //logCallStack = StackTraceUtility.ExtractStackTrace();
    }

    public Log(GameDate date, string message, ActualGoapNode goapAction = null) {
        persistentID = UtilityScripts.Utilities.GetNewUniqueID();
        this.id = UtilityScripts.Utilities.SetID(this);
        this.month = (MONTH)date.month;
        this.day = date.day;
        this.year = date.year;
        this.tick = date.tick;
        this.message = message;
        this._node = goapAction;
        this.fillers = new List<LogFiller>();
        //this.lockFillers = false;
        logText = string.Empty;
        //logCallStack = StackTraceUtility.ExtractStackTrace();
    }

    public Log(SaveDataLog data) {
        persistentID = data.persistentID;
        id = UtilityScripts.Utilities.SetID(this, data.id);
        month = data.month;
        day = data.day;
        year = data.year;
        tick = data.tick;

        category = data.category;
        file = data.file;
        key = data.key;

        message = data.message;
        logText = data.logText;
    }

    public void SetLogType(LOG_TYPE logType) {
        this.logType = logType;
    }

    internal void AddToFillers(object obj, string value, LOG_IDENTIFIER identifier, bool replaceExisting = true){
        //if (lockFillers) {
        //    return;
        //}
        if (replaceExisting && HasFillerForIdentifier(identifier)) {
            fillers.Remove(GetFillerForIdentifier(identifier));
        }
		this.fillers.Add (new LogFiller (obj, value, identifier));
	}
    internal void AddToFillers(LogFiller filler, bool replaceExisting = true) {
        AddToFillers(filler.obj, filler.value, filler.identifier, replaceExisting);
    }
    internal void AddToFillers(List<LogFiller> fillers, bool replaceExisting = true) {
        for (int i = 0; i < fillers.Count; i++) {
            LogFiller filler = fillers[i];
            AddToFillers(filler);
        }
    }
    public void SetFillers(List<LogFiller> fillers) {
        //if (lockFillers) {
        //    return;
        //}
        this.fillers = fillers;
    }
    public void AddLogToInvolvedObjects() {
        for (int i = 0; i < fillers.Count; i++) {
            LogFiller currFiller = fillers[i];
            object obj = currFiller.obj;
            if (obj != null) {
                if (obj is IPointOfInterest pointOfInterest) {
                    if (pointOfInterest.CollectsLogs()) {
                        pointOfInterest.logComponent.AddHistory(this);    
                    }
                } else if (obj is Faction faction) {
                    faction.AddHistory(this);
                }
            }
        }
    }
    public void UpdateLogInInvolvedObjects() {
        for (int i = 0; i < fillers.Count; i++) {
            LogFiller currFiller = fillers[i];
            object obj = currFiller.obj;
            if (obj != null) {
                if (obj is IPointOfInterest pointOfInterest) {
                    if (pointOfInterest.CollectsLogs()) {
                        SetLogText(fillers.Count > 0 ? UtilityScripts.Utilities.LogReplacer(this, true) 
                            : LocalizationManager.Instance.GetLocalizedValue(category, file, key));
                        Messenger.Broadcast(Signals.UPDATE_POI_LOGS_UI, pointOfInterest);    
                    }
                } else if (obj is Faction faction) {
                    Messenger.Broadcast(Signals.UPDATE_FACTION_LOGS_UI, faction);
                    SetLogText(fillers.Count > 0 ? UtilityScripts.Utilities.LogReplacer(this, true) 
                        : LocalizationManager.Instance.GetLocalizedValue(category, file, key));
                }
            }
        }
    }
    public void AddLogToSpecificObjects(params LOG_IDENTIFIER[] identifiers) {
        //List<LOG_IDENTIFIER> identifiersList = identifiers.ToList();
        for (int i = 0; i < fillers.Count; i++) {
            LogFiller currFiller = fillers[i];
            object obj = currFiller.obj;
            if (obj != null && identifiers.Contains(currFiller.identifier)) {
                if (obj is IPointOfInterest) {
                    (obj as IPointOfInterest).logComponent.AddHistory(this);
                } 
                //else if (obj is NPCSettlement) {
                //    (obj as NPCSettlement).AddHistory(this);
                //} 
                //else if (obj is Minion) {
                //    (obj as Minion).character.AddHistory(this);
                //} 
                else if (obj is Faction) {
                    (obj as Faction).AddHistory(this);
                }
            }
        }
    }
    public bool HasFillerForIdentifier(LOG_IDENTIFIER identifier) {
        for (int i = 0; i < fillers.Count; i++) {
            LogFiller currFiller = fillers[i];
            if (currFiller.identifier == identifier) {
                return true;
            }
        }
        return false;
    }
    public LogFiller GetFillerForIdentifier(LOG_IDENTIFIER identifier) {
        for (int i = 0; i < fillers.Count; i++) {
            LogFiller currFiller = fillers[i];
            if (currFiller.identifier == identifier) {
                return currFiller;
            }
        }
        return default(LogFiller);
    }
    public bool IsIncludedInFillers(object obj) {
        for (int i = 0; i < fillers.Count; i++) {
            if (fillers[i].obj == obj) {
                return true;
            }
        }
        return false;
    }
    public bool HasFillerThatMeetsRequirement(System.Func<object, bool> requirement) {
        for (int i = 0; i < fillers.Count; i++) {
            LogFiller filler = fillers[i];
            if (filler.obj != null && requirement.Invoke(filler.obj)) {
                return true;
            }
        }
        return false;
    } 
    private ActualGoapNode GetNodeAssociatedWithThisLog() {
        //if(_node != null) {
        //    if(_node.goapType == INTERACTION_TYPE.SHARE_INFORMATION) {
        //        //return (_goapAction as ShareInformation).eventToBeShared;
        //    }
        //}
        return _node;
    }

    #region Utilities
    //public void SetFillerLockedState(bool state) {
    //    lockFillers = state;
    //}
    public string GetLogDebugInfo() {
        //if (fromInteraction.intel != null) {
        //    return fromInteraction.intel.GetDebugInfo();
        //}
        return string.Empty;
    }
    public void SetDate(GameDate date) {
        this.month = (MONTH)date.month;
        this.day = date.day;
        this.year = date.year;
        this.tick = date.tick;
    }
    public void SetLogText(string text) {
        this.logText = text;
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataLog data) {
        fillers = new List<LogFiller>();
        for (int i = 0; i < data.fillers.Count; i++) {
            LogFiller filler = data.fillers[i].Load();
            fillers.Add(filler);
        }

        //No goap action when loaded because we cannot save goap action
        if(data.actionID != string.Empty) {
            _node = DatabaseManager.Instance.actionDatabase.GetActionByPersistentID(data.actionID);
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataLog : SaveData<Log>, ISavableCounterpart {
    public string persistentID { get; set; }
    public int id;

    public MONTH month;
    public int day;
    public int year;
    public int tick;

    public string category;
    public string file;
    public string key;

    public string message;
    public string logText;

    public List<SaveDataLogFiller> fillers;
    public string actionID;

    #region getters
    public OBJECT_TYPE objectType => OBJECT_TYPE.Log;
    #endregion

    #region Overrides
    public override void Save(Log data) {
        persistentID = data.persistentID;
        id = data.id;
        month = data.month;
        day = data.day;
        year = data.year;
        tick = data.tick;

        category = data.category;
        file = data.file;
        key = data.key;

        message = data.message;
        logText = data.logText;

        fillers = new List<SaveDataLogFiller>();
        for (int i = 0; i < data.fillers.Count; i++) {
            SaveDataLogFiller filler = new SaveDataLogFiller();
            filler.Save(data.fillers[i]);
            fillers.Add(filler);
        }

        ActualGoapNode node = data.node;
        if(node != null) {
            actionID = node.persistentID;
        }
    }

    public override Log Load() {
        return new Log(this);
    }
    #endregion
}