using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatheringComponent : CharacterComponent {
    public Gathering currentGathering { get; private set; }

    #region getters
    public bool hasGathering => currentGathering != null;
    #endregion

    public GatheringComponent() {
    }

    public GatheringComponent(SaveDataGatheringComponent data) {
    }

    #region General
    public void SetCurrentGathering(Gathering gathering) {
        currentGathering = gathering;
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataGatheringComponent data) {
        if (!string.IsNullOrEmpty(data.currentGathering)) {
            currentGathering = DatabaseManager.Instance.gatheringDatabase.GetGatheringByPersistentID(data.currentGathering);
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataGatheringComponent : SaveData<GatheringComponent> {
    public string currentGathering;

    #region Overrides
    public override void Save(GatheringComponent data) {
        if (data.hasGathering) {
            currentGathering = data.currentGathering.persistentID;
            SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.currentGathering);
        }
    }

    public override GatheringComponent Load() {
        GatheringComponent component = new GatheringComponent(this);
        return component;
    }
    #endregion
}