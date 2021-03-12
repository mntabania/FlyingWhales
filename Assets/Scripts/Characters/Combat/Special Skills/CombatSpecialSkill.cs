using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatSpecialSkill {
    public COMBAT_SPECIAL_SKILL specialSkillType { get; private set; }
    public COMBAT_SPECIAL_SKILL_TARGET targetType { get; private set; }
    public int cooldownInTicks { get; private set; } //0 means no cooldown
    public string name { get; private set; }

    public CombatSpecialSkill(COMBAT_SPECIAL_SKILL p_specialSkillType, COMBAT_SPECIAL_SKILL_TARGET p_targetType, int p_cooldownInTicks) {
        name = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(p_specialSkillType.ToString());
        specialSkillType = p_specialSkillType;
        targetType = p_targetType;
        cooldownInTicks = p_cooldownInTicks;
    }

    #region Virtuals
    public virtual void SetSpecialSkill(Character p_character) { }
    public virtual void UnsetSpecialSkill(Character p_character) { }
    public virtual bool TryActivateSkill(Character p_character) { return false; }

    protected virtual Character GetValidTargetFor(Character p_character) { return null; }
    protected virtual void PopulateValidTargetsFor(Character p_character, List<Character> p_validTargets) { }
    #endregion
}

public class CombatSpecialSkillWrapper {
    public CombatSpecialSkill specialSkill { get; private set; }
    public int currentCooldown { get; private set; }

    #region getters
    public bool isInCooldown => specialSkill != null && currentCooldown < specialSkill.cooldownInTicks;
    #endregion

    public CombatSpecialSkillWrapper() {
    }
    public CombatSpecialSkillWrapper(SaveDataCombatSpecialSkillWrapper data) {
        specialSkill = CombatManager.Instance.GetCombatSpecialSkill(data.specialSkill);
        currentCooldown = data.currentCooldown;
    }
    #region Listeners
    private void OnTickStarted() {
        PerTickCooldown();
    }
    #endregion

    #region Special Skill
    private void SetSpecialSkill(CombatSpecialSkill p_skill) {
        if(specialSkill != p_skill) {
            specialSkill = p_skill;
            if (specialSkill != null) {
                ResetCooldown();
            }
        }
    }
    public void SetSpecialSkill(COMBAT_SPECIAL_SKILL p_skillType) {
        CombatSpecialSkill skill = CombatManager.Instance.GetCombatSpecialSkill(p_skillType);
        SetSpecialSkill(skill);
    }
    public bool TryActivateSpecialSkill(Character p_character) {
        if (!isInCooldown) {
            if(specialSkill != null && specialSkill.TryActivateSkill(p_character)) {
                StartCooldown();
            }
        }
        return false;
    }
    public bool HasSpecialSkill() {
        return specialSkill != null;
    }
    #endregion

    #region Cooldown
    public void StartCooldown() {
        currentCooldown = 0;
        if(specialSkill.cooldownInTicks > 0) {
            Messenger.AddListener(Signals.TICK_STARTED, OnTickStarted);
        } else {
            currentCooldown = specialSkill.cooldownInTicks;
        }
    }
    private void PerTickCooldown() {
        currentCooldown++;
        if (!isInCooldown) {
            if(specialSkill != null) {
                currentCooldown = specialSkill.cooldownInTicks;
            }
            Messenger.RemoveListener(Signals.TICK_STARTED, OnTickStarted);
        }
    }
    private void ResetCooldown() {
        currentCooldown = specialSkill.cooldownInTicks;
    }
    #endregion

    #region Loading
    public void LoadReferences() {
        if (isInCooldown) {
            Messenger.AddListener(Signals.TICK_STARTED, OnTickStarted);
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataCombatSpecialSkillWrapper : SaveData<CombatSpecialSkillWrapper> {
    public COMBAT_SPECIAL_SKILL specialSkill { get; private set; }
    public int currentCooldown { get; private set; }

    public override void Save(CombatSpecialSkillWrapper data) {
        base.Save(data);
        specialSkill = COMBAT_SPECIAL_SKILL.None;
        if(data.specialSkill != null) {
            specialSkill = data.specialSkill.specialSkillType;
        }
        currentCooldown = data.currentCooldown;
    }
    public override CombatSpecialSkillWrapper Load() {
        return new CombatSpecialSkillWrapper(this);
    }
}