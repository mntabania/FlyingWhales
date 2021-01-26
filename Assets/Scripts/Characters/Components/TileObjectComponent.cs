using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;

public class TileObjectComponent : CharacterComponent {
    public Bed primaryBed { get; private set; }
    public BaseBed bedBeingUsed { get; private set; }

    #region getters
    public bool isUsingBed => bedBeingUsed != null;
    #endregion

    public TileObjectComponent() {
    }
    public TileObjectComponent(SaveDataTileObjectComponent data) {
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
    public void LoadReferences(SaveDataTileObjectComponent data) {
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
public class SaveDataTileObjectComponent : SaveData<TileObjectComponent> {
    public string primaryBed;
    public string bedBeingUsed;

    #region Overrides
    public override void Save(TileObjectComponent data) {
        if(data.primaryBed != null) {
            primaryBed = data.primaryBed.persistentID;
        }
        if (data.bedBeingUsed != null) {
            bedBeingUsed = data.bedBeingUsed.persistentID;
        }
    }

    public override TileObjectComponent Load() {
        TileObjectComponent component = new TileObjectComponent(this);
        return component;
    }
    #endregion
}