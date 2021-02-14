using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDataPlayerSkill : SaveData<SkillData> {
    public PLAYER_SKILL_TYPE type;
    public int maxCharges;
    public int charges;
    public int manaCost;
    public int cooldown;
    public int threat;
    public int threatPerHour;
    public int unlockCost;
    public int currentLevel;
    public List<int> upgradeCost;
    public int currentCooldownTick;
    public float basePierce;

    #region Overrides
    public override void Save(SkillData data) {
        type = data.type;
        maxCharges = data.baseMaxCharges;
        charges = data.charges;
        manaCost = data.baseManaCost;
        cooldown = data.baseCooldown;
        threat = data.baseThreat;
        threatPerHour = data.threatPerHour;
        currentCooldownTick = data.currentCooldownTick;
        currentLevel = data.currentLevel;
        basePierce = data.basePierce;
        unlockCost = data.unlockCost;

    }
    public override SkillData Load() {
        SkillData data = PlayerSkillManager.Instance.GetPlayerSkillData(type);
        data.SetCharges(charges);
        data.SetCooldown(cooldown);
        data.SetManaCost(manaCost);
        data.SetPierce(basePierce);
        data.SetThreat(threat);
        data.SetThreatPerHour(threatPerHour);
        data.SetUnlockCost(unlockCost);
        data.SetCurrentCooldownTick(currentCooldownTick);
        data.SetMaxCharges(maxCharges);
        data.SetCurrentLevel(currentLevel);
        return data;
    }
    #endregion
}
