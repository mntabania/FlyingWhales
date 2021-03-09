
public class SkillProgressionManager {

	public bool CheckAndUpgrade(CurrenciesComponent p_currencies, PLAYER_SKILL_TYPE p_type) {
		bool success = false;
		PlayerSkillData data = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(p_type);
		UnityEngine.Debug.Log(data);
		if (data.unlockCost <= p_currencies.Mana) {
			success = true;
		}
		UnityEngine.Debug.Log("Unlocking " + data + ": " + success);
		return success;
	}

	public int CheckRequirementsAndGetUnlockCost(PlayerSkillComponent p_skills, CurrenciesComponent p_currencies, PLAYER_SKILL_TYPE p_type) {
		int unlockCost = CheckRequirement(p_skills, p_currencies.Mana, p_type);
		return unlockCost;
	}

	public int CheckRequirementsAndGetUnlockCost(PlayerSkillComponent p_skills, int p_mana, PLAYER_SKILL_TYPE p_type) {
		int unlockCost = CheckRequirement(p_skills, p_mana, p_type);
		return unlockCost;
	}

	private int CheckRequirement(PlayerSkillComponent p_availablePlayerSkills, int p_mana, PLAYER_SKILL_TYPE p_type) {
		PlayerSkillData playerSkilldata = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(p_type);

		for (int x = 0; x < playerSkilldata.requirementData.requiredSkills.Count; ++x) {
			if (!p_availablePlayerSkills.CheckIfSkillIsAvailable(playerSkilldata.requirementData.requiredSkills[x])) {
				return -1;
			}
		}
		if (playerSkilldata.requirementData.actionCount <= p_availablePlayerSkills.playerActions.Count &&
			playerSkilldata.requirementData.afflictionCount <= p_availablePlayerSkills.afflictions.Count &&
			playerSkilldata.requirementData.spellsCount <= p_availablePlayerSkills.spells.Count &&
			playerSkilldata.requirementData.tier1Count <= p_availablePlayerSkills.tier1Count &&
			playerSkilldata.requirementData.tier2Count <= p_availablePlayerSkills.tier2Count &&
			playerSkilldata.requirementData.tier3Count <= p_availablePlayerSkills.tier3Count) {
			return playerSkilldata.unlockCost;
		}
		return -1;
	}
}
