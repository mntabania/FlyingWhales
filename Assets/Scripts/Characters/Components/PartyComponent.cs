using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyComponent : CharacterComponent {
    public Party currentParty { get; private set; }

    #region getters
    public bool hasParty => currentParty != null;
    #endregion

    public PartyComponent() {
    }

    public PartyComponent(SaveDataPartyComponent data) {
    }

    public void SetCurrentParty(Party party) {
        currentParty = party;
    }

    #region Loading
    public void LoadReferences(SaveDataPartyComponent data) {
        if (!string.IsNullOrEmpty(data.currentParty)) {
            currentParty = DatabaseManager.Instance.partyDatabase.GetPartyByPersistentID(data.currentParty);
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataPartyComponent : SaveData<PartyComponent> {
    public string currentParty;

    #region Overrides
    public override void Save(PartyComponent data) {
        if (data.currentParty != null) {
            currentParty = data.currentParty.persistentID;
            SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.currentParty);
        }
    }

    public override PartyComponent Load() {
        PartyComponent component = new PartyComponent(this);
        return component;
    }
    #endregion
}