using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;

public class TileObjectComponent : CharacterComponent {
    public Bed primaryBed { get; private set; }

    public TileObjectComponent() {
    }
    public TileObjectComponent(SaveDataTileObjectComponent data) {
    }

    #region General
    public void SetPrimaryBed(Bed bed) {
        primaryBed = bed;
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataTileObjectComponent data) {
        if (!string.IsNullOrEmpty(data.primaryBed)) {
            primaryBed = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(data.primaryBed) as Bed;
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataTileObjectComponent : SaveData<TileObjectComponent> {
    public string primaryBed;

    #region Overrides
    public override void Save(TileObjectComponent data) {
        if(data.primaryBed != null) {
            primaryBed = data.primaryBed.persistentID;
        }
    }

    public override TileObjectComponent Load() {
        TileObjectComponent component = new TileObjectComponent(this);
        return component;
    }
    #endregion
}