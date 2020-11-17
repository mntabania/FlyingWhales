using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlagueComponent {
    private int _plaguePoints;

    #region getters
    public int plaguePoints => _plaguePoints;
    #endregion

    public PlagueComponent() {
        _plaguePoints = 1000;
    }
    public PlagueComponent(SaveDataPlagueComponent p_component) {
        _plaguePoints = p_component.plaguePoints;
    }

    #region Plague Points
    public void AdjustPlaguePoints(int amount) {
        _plaguePoints += amount;
        Messenger.Broadcast(PlayerSignals.UPDATED_PLAGUE_POINTS, _plaguePoints);
    }
    public void GainPlaguePointFromCharacter(int amount, Character p_character) {
        AdjustPlaguePoints(amount);
        PlayerUI.Instance.ShowPlaguePointsGainedEffect(amount, p_character.worldPosition);
    }
    #endregion
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