using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;

public class CharacterTileObjectComponent : CharacterComponent {
    public Bed primaryBed { get; private set; }
    public BaseBed bedBeingUsed { get; private set; }

    #region getters
    public bool isUsingBed => bedBeingUsed != null;
    #endregion

    public CharacterTileObjectComponent() {
    }
    public CharacterTileObjectComponent(SaveDataCharacterTileObjectComponent data) {
    }

    #region General
    public void SetPrimaryBed(Bed bed) {
        primaryBed = bed;
    }
    public void SetBedBeingUsed(BaseBed p_bed) {
        bedBeingUsed = p_bed;
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataCharacterTileObjectComponent data) {
        if (!string.IsNullOrEmpty(data.primaryBed)) {
            primaryBed = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(data.primaryBed) as Bed;
        }
        if (!string.IsNullOrEmpty(data.bedBeingUsed)) {
            bedBeingUsed = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(data.bedBeingUsed) as BaseBed;
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataCharacterTileObjectComponent : SaveData<CharacterTileObjectComponent> {
    public string primaryBed;
    public string bedBeingUsed;

    #region Overrides
    public override void Save(CharacterTileObjectComponent data) {
        if(data.primaryBed != null) {
            primaryBed = data.primaryBed.persistentID;
        }
        if (data.bedBeingUsed != null) {
            bedBeingUsed = data.bedBeingUsed.persistentID;
        }
    }

    public override CharacterTileObjectComponent Load() {
        CharacterTileObjectComponent component = new CharacterTileObjectComponent(this);
        return component;
    }
    #endregion
}