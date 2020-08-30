using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BaseSaveDataHub {
    public virtual bool AddToSave<T>(T data) {
        return false;
    }
    public virtual bool RemoveFromSave<T>(T data) {
        return false;
    }
    public virtual ISavableCounterpart GetData(string persistendID) {
        return default;
    }
}

[System.Serializable]
public class SaveDataFactionHub : BaseSaveDataHub {
    public Dictionary<string, SaveDataFaction> _hub;

    #region getters
    public Dictionary<string, SaveDataFaction> hub => _hub;
    #endregion

    public SaveDataFactionHub() {
        _hub = new Dictionary<string, SaveDataFaction>();
    }

    public override bool AddToSave<T>(T data) {
        if (data is SaveDataFaction save) {
            if (!_hub.ContainsKey(save.persistentID)) {
                _hub.Add(save.persistentID, save);
                return true;
            }
        }
        return false;
    }

    public override bool RemoveFromSave<T>(T data) {
        if (data is SaveDataFaction save) {
            return _hub.Remove(save.persistentID);
        }
        return false;
    }
    public override ISavableCounterpart GetData(string persistendID) {
        if (_hub.ContainsKey(persistendID)) {
            return _hub[persistendID];
        }
        return default;
    }
}