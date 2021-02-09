
public class SkillProgressionManager {

	public bool CheckAndUnlock(CurrenciesComponent p_currencies, SkillData p_targetSkillData) {
		bool success = false;
		PlayerSkillData data = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(p_targetSkillData.type);
		UnityEngine.Debug.Log(data);
		if (data.unlockCost <= p_currencies.Mana) {
			success = true;
		}
		UnityEngine.Debug.Log("Unlocking " + p_targetSkillData.name + ": " + success);
		return success;
	}

	public bool CheckAndUpgrade(PlayerSkillComponent p_skills, CurrenciesComponent p_currencies, SkillData p_targetSkillData) {
		bool success = false;
		if (CheckRequirement(p_skills, p_currencies, p_targetSkillData)) {
			success = true;
		}
		UnityEngine.Debug.Log("Upgrading " + p_targetSkillData.name + ": " + success);
		return success;
	}

	private bool CheckRequirement(PlayerSkillComponent p_availablePlayerSkills, CurrenciesComponent p_currencies, SkillData p_targetSkillData) {
		PlayerSkillData playerSkilldata = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(p_targetSkillData.type);

		for (int x = 0; x < playerSkilldata.requirementData.requiredSkills.Count; ++x) {
			if (!p_availablePlayerSkills.CheckIfSkillIsAvailable(playerSkilldata.requirementData.requiredSkills[x])) {
				return false;
			}
		}
		if (playerSkilldata.requirementData.actionCount <= p_availablePlayerSkills.playerActions.Count &&
			playerSkilldata.requirementData.afflictionCount <= p_availablePlayerSkills.afflictions.Count &&
			playerSkilldata.requirementData.spellsCount <= p_availablePlayerSkills.spells.Count &&
			playerSkilldata.requirementData.tier1Count <= p_availablePlayerSkills.tier1Count &&
			playerSkilldata.requirementData.tier2Count <= p_availablePlayerSkills.tier2Count &&
			playerSkilldata.requirementData.tier3Count <= p_availablePlayerSkills.tier3Count &&
			playerSkilldata.skillUpgradeData.upgradeCosts[p_availablePlayerSkills.GetLevelOfSkill(p_targetSkillData)] <= p_currencies.ChaoticEnergy) {

			return true;
		}
		return false;
	}
}
