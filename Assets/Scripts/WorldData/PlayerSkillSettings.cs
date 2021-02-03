using System;
using UnityEngine;

[System.Serializable]
public class PlayerSkillSettings {
    public SKILL_COOLDOWN_SPEED cooldownSpeed;
    public SKILL_COST_AMOUNT costAmount;
    public SKILL_CHARGE_AMOUNT chargeAmount;
    public THREAT_AMOUNT threatAmount;
    /// <summary>
    /// The forced archetype setting.
    /// If this is set to Normal, it means that the player can choose between the Pre-set archetypes. 
    /// </summary>
    public PLAYER_ARCHETYPE forcedArchetype;
    public OMNIPOTENT_MODE omnipotentMode;
    
    public PlayerSkillSettings() {
        cooldownSpeed = SKILL_COOLDOWN_SPEED.Normal;
        costAmount = SKILL_COST_AMOUNT.Normal;
        chargeAmount = SKILL_CHARGE_AMOUNT.Normal;
        threatAmount = THREAT_AMOUNT.Normal;
        forcedArchetype = PLAYER_ARCHETYPE.Normal;
        omnipotentMode = OMNIPOTENT_MODE.Disabled;
    }
    public void SetCooldownSpeed(SKILL_COOLDOWN_SPEED p_value) {
        cooldownSpeed = p_value;
        Debug.Log($"Set Cooldown Speed {p_value.ToString()}");
    }
    public float GetCooldownSpeedModification() {
        switch (cooldownSpeed) {
            case SKILL_COOLDOWN_SPEED.None:
                return 0f;
            case SKILL_COOLDOWN_SPEED.Half:
                return 0.5f;
            case SKILL_COOLDOWN_SPEED.Normal:
                return 1f;
            case SKILL_COOLDOWN_SPEED.Double:
                return 2f;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public void SetManaCostAmount(SKILL_COST_AMOUNT p_value) {
        costAmount = p_value;
        Debug.Log($"Set Skill Cost {p_value.ToString()}");
    }
    public float GetCostsModification() {
        switch (costAmount) {
            case SKILL_COST_AMOUNT.None:
                return 0f;
            case SKILL_COST_AMOUNT.Half:
                return 0.5f;
            case SKILL_COST_AMOUNT.Normal:
                return 1f;
            case SKILL_COST_AMOUNT.Double:
                return 2f;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public void SetChargeAmount(SKILL_CHARGE_AMOUNT p_value) {
        chargeAmount = p_value;
        Debug.Log($"Set Charge Amount {p_value.ToString()}");
    }
    public float GetChargeCostsModification() {
        switch (chargeAmount) {
            case SKILL_CHARGE_AMOUNT.Unlimited:
            case SKILL_CHARGE_AMOUNT.Normal:
                return 1f;
            case SKILL_CHARGE_AMOUNT.Half:
                return 0.5f;
            case SKILL_CHARGE_AMOUNT.Double:
                return 2f;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public void SetThreatAmount(THREAT_AMOUNT p_value) {
        threatAmount = p_value;
        Debug.Log($"Set Threat Amount {p_value.ToString()}");
    }
    public float GetThreatModification() {
        switch (threatAmount) {
            case THREAT_AMOUNT.None:
                return 0f;
            case THREAT_AMOUNT.Half:
                return 0.5f;
            case THREAT_AMOUNT.Normal:
                return 1f;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    #region Forced Archetype
    public void SetForcedArchetype(PLAYER_ARCHETYPE p_archetype) {
        forcedArchetype = p_archetype;
    }
    #endregion

    #region Omnipotent Mode
    public void SetOmnipotentMode(OMNIPOTENT_MODE p_omnipotentMode) {
        omnipotentMode = p_omnipotentMode;
    }
    #endregion
}