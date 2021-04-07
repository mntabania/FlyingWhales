using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDataSkillData : SaveData<SkillData> {
    public PLAYER_SKILL_TYPE type;
    public int maxCharges;
    public int charges;
    public int bonusCharges;
    public int manaCost;
    public int cooldown;
    public int threat;
    public int threatPerHour;
    public int unlockCost;
    public int currentLevel;
    public int currentCooldownTick;
    public float basePierce;
    public bool isUnlockedBaseOnRequirements;
    public bool isInUse;
    public bool isTemporarilyInUse;

    #region Overrides
    public override void Save(SkillData data) {
        type = data.type;
        maxCharges = data.baseMaxCharges;
        charges = data.charges;
        bonusCharges = data.bonusCharges;
        manaCost = data.baseManaCost;
        cooldown = data.baseCooldown;
        threat = data.baseThreat;
        threatPerHour = data.threatPerHour;
        currentCooldownTick = data.currentCooldownTick;
        currentLevel = data.currentLevel;
        basePierce = data.basePierce;
        unlockCost = data.unlockCost;
        isUnlockedBaseOnRequirements = data.isUnlockedBaseOnRequirements;
        isInUse = data.isInUse;
        isTemporarilyInUse = data.isTemporarilyInUse;
    }
    public override SkillData Load() {
        SkillData data = PlayerSkillManager.Instance.GetSkillData(type);
        data.SetCharges(charges);
        data.SetBonusCharges(bonusCharges);
        data.SetCooldown(cooldown);
        data.SetManaCost(manaCost);
        data.SetPierce(basePierce);
        data.SetThreat(threat);
        data.SetThreatPerHour(threatPerHour);
        data.SetUnlockCost(unlockCost);
        data.SetCurrentCooldownTick(currentCooldownTick);
        data.SetMaxCharges(maxCharges);
        data.SetCurrentLevel(currentLevel);
        data.SetIsUnlockBaseOnRequirements(isUnlockedBaseOnRequirements);
        data.SetIsInUse(isInUse);
        data.SetIsTemporarilyInUse(isTemporarilyInUse);
        return data;
    }
    #endregion
}
