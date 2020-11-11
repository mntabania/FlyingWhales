using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlagueComponent {
    private int _plaguePoints;

    #region getters
    public int plaguePoints => _plaguePoints;
    #endregion

    public PlagueComponent() {
    }
    public PlagueComponent(SaveDataPlagueComponent p_component) {
        _plaguePoints = p_component.plaguePoints;
    }
}

[System.Serializable]
public class SaveDataPlagueComponent : SaveData<PlagueComponent> {
    public int plaguePoints;

    public override void Save(PlagueComponent p_component) {
        plaguePoints = p_component.plaguePoints;
    }
    public override PlagueComponent Load() {
        PlagueComponent component = new PlagueComponent();
        return component;
    }
}