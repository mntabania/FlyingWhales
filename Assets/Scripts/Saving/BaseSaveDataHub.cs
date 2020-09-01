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
    public virtual ISavableCounterpart GetData(string persistentID) {
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
    public override ISavableCounterpart GetData(string persistentID) {
        if (_hub.ContainsKey(persistentID)) {
            return _hub[persistentID];
        }
        return default;
    }
}

[System.Serializable]
public class SaveDataTileObjectHub : BaseSaveDataHub {
    public Dictionary<string, SaveDataTileObject> _hub;

    #region getters
    public Dictionary<string, SaveDataTileObject> hub => _hub;
    #endregion

    public SaveDataTileObjectHub() {
        _hub = new Dictionary<string, SaveDataTileObject>();
    }

    public override bool AddToSave<T>(T data) {
        if (data is SaveDataTileObject save) {
            if (!_hub.ContainsKey(save.persistentID)) {
                _hub.Add(save.persistentID, save);
                return true;
            }
        }
        return false;
    }

    public override bool RemoveFromSave<T>(T data) {
        if (data is SaveDataTileObject save) {
            return _hub.Remove(save.persistentID);
        }
        return false;
    }
    public override ISavableCounterpart GetData(string persistentID) {
        if (_hub.ContainsKey(persistentID)) {
            return _hub[persistentID];
        }
        return default;
    }
}

[System.Serializable]
public class SaveDataLogHub : BaseSaveDataHub {
    public Dictionary<string, SaveDataLog> _hub;

    #region getters
    public Dictionary<string, SaveDataLog> hub => _hub;
    #endregion

    public SaveDataLogHub() {
        _hub = new Dictionary<string, SaveDataLog>();
    }

    public override bool AddToSave<T>(T data) {
        if (data is SaveDataLog save) {
            if (!_hub.ContainsKey(save.persistentID)) {
                _hub.Add(save.persistentID, save);
                return true;
            }
        }
        return false;
    }

    public override bool RemoveFromSave<T>(T data) {
        if (data is SaveDataLog save) {
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

[System.Serializable]
public class SaveDataTraitHub : BaseSaveDataHub {
    public Dictionary<string, SaveDataTrait> _hub;

    #region getters
    public Dictionary<string, SaveDataTrait> hub => _hub;
    #endregion

    public SaveDataTraitHub() {
        _hub = new Dictionary<string, SaveDataTrait>();
    }

    public override bool AddToSave<T>(T data) {
        if (data is SaveDataTrait save) {
            if (!_hub.ContainsKey(save.persistentID)) {
                _hub.Add(save.persistentID, save);
                return true;
            }
        }
        return false;
    }

    public override bool RemoveFromSave<T>(T data) {
        if (data is SaveDataTrait save) {
            return _hub.Remove(save.persistentID);
        }
        return false;
    }
    public override ISavableCounterpart GetData(string persistentID) {
        if (_hub.ContainsKey(persistentID)) {
            return _hub[persistentID];
        }
        return default;
    }
}

[System.Serializable]
public class SaveDataJobHub : BaseSaveDataHub {
    public Dictionary<string, SaveDataJobQueueItem> _hub;

    #region getters
    public Dictionary<string, SaveDataJobQueueItem> hub => _hub;
    #endregion

    public SaveDataJobHub() {
        _hub = new Dictionary<string, SaveDataJobQueueItem>();
    }

    public override bool AddToSave<T>(T data) {
        if (data is SaveDataJobQueueItem save) {
            if (!_hub.ContainsKey(save.persistentID)) {
                _hub.Add(save.persistentID, save);
                return true;
            }
        }
        return false;
    }

    public override bool RemoveFromSave<T>(T data) {
        if (data is SaveDataJobQueueItem save) {
            return _hub.Remove(save.persistentID);
        }
        return false;
    }
    public override ISavableCounterpart GetData(string persistentID) {
        if (_hub.ContainsKey(persistentID)) {
            return _hub[persistentID];
        }
        return default;
    }
}

[System.Serializable]
public class SaveDataSettlementHub : BaseSaveDataHub {
    public Dictionary<string, SaveDataBaseSettlement> _hub;

    #region getters
    public Dictionary<string, SaveDataBaseSettlement> hub => _hub;
    #endregion

    public SaveDataSettlementHub() {
        _hub = new Dictionary<string, SaveDataBaseSettlement>();
    }

    public override bool AddToSave<T>(T data) {
        if (data is SaveDataBaseSettlement save) {
            if (!_hub.ContainsKey(save._persistentID)) {
                _hub.Add(save._persistentID, save);
                return true;
            }
        }
        return false;
    }

    public override bool RemoveFromSave<T>(T data) {
        if (data is SaveDataBaseSettlement save) {
            return _hub.Remove(save._persistentID);
        }
        return false;
    }
    public override ISavableCounterpart GetData(string persistentID) {
        if (_hub.ContainsKey(persistentID)) {
            return _hub[persistentID];
        }
        return default;
    }
}