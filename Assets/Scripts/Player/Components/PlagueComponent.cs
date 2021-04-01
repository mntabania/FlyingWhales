﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlagueComponent {
    private int _plaguePoints;

    #region getters
    public int plaguePoints => _plaguePoints;
    public int maxPlaguePoints { private set; get; }
    #endregion

    public PlagueComponent() {
        _plaguePoints = 50;
        maxPlaguePoints = EditableValuesManager.Instance.GetInitialMaxChaoticEnergy();
        Messenger.AddListener<int>(PlayerSignals.PLAYER_FINISHED_PORTAL_UPGRADE, OnPortalUpgraded);
    }
    public PlagueComponent(SaveDataPlagueComponent p_component) {
        _plaguePoints = p_component.plaguePoints;
        Messenger.AddListener<int>(PlayerSignals.PLAYER_FINISHED_PORTAL_UPGRADE, OnPortalUpgraded);
    }

    #region Plague Points
    void OnPortalUpgraded(int p_currentPortalLevel) {
        maxPlaguePoints = EditableValuesManager.Instance.GetMaxChaoticEnergyPerPortalLevel(p_currentPortalLevel);
    }
    public void AdjustPlaguePoints(int amount) {
        if (WorldSettings.Instance != null && WorldSettings.Instance.worldSettingsData.playerSkillSettings.costAmount == SKILL_COST_AMOUNT.None) {
            return;
        }
        _plaguePoints = Mathf.Clamp(_plaguePoints + amount, 0, maxPlaguePoints);

        Messenger.Broadcast(PlayerSignals.UPDATED_PLAGUE_POINTS, _plaguePoints);
    }
    public void GainPlaguePointFromCharacter(int amount, Character p_character) {
        return;//remove once blanacing is settled
        AdjustPlaguePoints(amount);
        PlayerUI.Instance.ShowPlaguePointsGainedEffect(amount);    
    }
    public bool CanGainPlaguePoints() {
        return false;//remove once blanacing is settled
        //return PlayerManager.Instance.player.playerSettlement != null && PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.BIOLAB);
    }
    #endregion
}

[System.Serializable]
public class SaveDataPlagueComponent : SaveData<PlagueComponent> {
    public int plaguePoints;
    public int maxPlaguePoints; 

    public override void Save(PlagueComponent p_component) {
        plaguePoints = p_component.plaguePoints;
        maxPlaguePoints = p_component.maxPlaguePoints;
    }
    public override PlagueComponent Load() {
        PlagueComponent component = new PlagueComponent(this);
        return component;
    }
}