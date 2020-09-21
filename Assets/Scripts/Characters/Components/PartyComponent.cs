using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyComponent : CharacterComponent {
    public Party currentParty { get; private set; }

    #region getters
    public bool hasParty => currentParty != null;
    public bool isActiveMember => IsPartyActiveAndOwnerActivePartOfQuest();
    #endregion

    public PartyComponent() {
    }

    public PartyComponent(SaveDataPartyComponent data) {
    }

    #region General
    public void SetCurrentParty(Party party) {
        currentParty = party;
    }
    private bool IsPartyActiveAndOwnerActivePartOfQuest() {
        if (hasParty) {
            return currentParty.isActive && currentParty.DidMemberJoinQuest(owner) && currentParty.IsMemberActive(owner);
        }
        return false;
    }
    public bool IsAMemberOfParty(Party party) {
        return currentParty == party;
    }
    #endregion

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
        if (data.hasParty) {
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