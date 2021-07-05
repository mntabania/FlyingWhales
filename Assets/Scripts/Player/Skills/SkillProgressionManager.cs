using Inner_Maps.Location_Structures;

public class SkillProgressionManager {

	public bool CheckAndUpgrade(CurrenciesComponent p_currencies, PLAYER_SKILL_TYPE p_type) {
		bool success = false;
		PlayerSkillData data = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(p_type);
#if DEBUG_LOG
		UnityEngine.Debug.Log(data);
#endif
		if (data.GetUnlockCost() <= p_currencies.Mana) {
			success = true;
		}
#if DEBUG_LOG
		UnityEngine.Debug.Log("Unlocking " + data + ": " + success);
#endif
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
		PlayerSkillData playerSkilldata = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(p_type);
		for (int x = 0; x < playerSkilldata.requirementData.requiredSkills.Count; ++x) {
			if (!p_availablePlayerSkills.CheckIfSkillIsAvailable(playerSkilldata.requirementData.requiredSkills[x])) {
				return -1;
			}
		}
		if (playerSkilldata.tier <= 0) {
			return -1;
		}
		if (playerSkilldata.requirementData.requiredArchetypes.Count > 0 && !playerSkilldata.requirementData.requiredArchetypes.Contains(PlayerSkillManager.Instance.selectedArchetype)) {
			return -1;
		}
		/*
		if (playerSkilldata.requirementData.actionCount <= p_availablePlayerSkills.playerActions.Count &&
			playerSkilldata.requirementData.afflictionCount <= p_availablePlayerSkills.afflictions.Count &&
			playerSkilldata.requirementData.spellsCount <= p_availablePlayerSkills.spells.Count &&
			playerSkilldata.requirementData.tier1Count <= p_availablePlayerSkills.tier1Count &&
			playerSkilldata.requirementData.tier2Count <= p_availablePlayerSkills.tier2Count &&
			playerSkilldata.requirementData.tier3Count <= p_availablePlayerSkills.tier3Count &&
			playerSkilldata.requirementData.portalLevel <= (PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal).level) {
			return playerSkilldata.unlockCost;
		}*/
		if (playerSkilldata.requirementData.portalLevel <= (PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal).level) {
			return playerSkilldata.GetUnlockCost();
		}
		// UnityEngine.Debug.LogError("D");
		return -1;
	}
}
