using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CurrenciesComponent {
    private int m_mana;
    private int m_chaoticEnergy;
    private int m_spirits;
    #region getters
    public int Mana => m_mana;
    public int ChaoticEnergy => m_chaoticEnergy;
    public int Spirits => m_spirits;
    #endregion

    public CurrenciesComponent() {
        m_mana = 35;
        m_chaoticEnergy = 35;
        m_spirits = 35;
    }
    public CurrenciesComponent(SaveDataCurrenciesComponent p_component) {
        m_mana = p_component.mana;
        m_chaoticEnergy = p_component.chaoticEnergy;
        m_spirits = p_component.spirits;
    }

    #region Plague Points
    public void AdjustPlaguePoints(int amount) {
        if (WorldSettings.Instance != null && WorldSettings.Instance.worldSettingsData.playerSkillSettings.costAmount == SKILL_COST_AMOUNT.None) {
            return;
        }
        m_mana += amount;
        Messenger.Broadcast(PlayerSignals.UPDATED_PLAGUE_POINTS, m_mana);
    }
    public void GainPlaguePointFromCharacter(int amount, Character p_character) {
        AdjustPlaguePoints(amount);
        // PlayerUI.Instance.ShowPlaguePointsGainedEffect(amount);
    }
    public bool CanGainPlaguePoints() {
        return PlayerManager.Instance.player.playerSettlement != null && PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.BIOLAB);
    }
    #endregion
}

[System.Serializable]
public class SaveDataCurrenciesComponent : SaveData<CurrenciesComponent> {
    public int mana;
    public int chaoticEnergy;
    public int spirits;

    public override void Save(CurrenciesComponent p_component) {
        mana = p_component.Mana;
        chaoticEnergy = p_component.ChaoticEnergy;
        spirits = p_component.Spirits;
    }
    public override CurrenciesComponent Load() {
        CurrenciesComponent component = new CurrenciesComponent(this);
        return component;
    }
}