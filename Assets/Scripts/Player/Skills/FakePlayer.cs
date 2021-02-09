using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakePlayer : MonoBehaviour
{
	public CurrenciesComponent currenciesComponent = new CurrenciesComponent();
    public PlayerSkillComponent skillComponent = new PlayerSkillComponent();

	public List<PLAYER_SKILL_TYPE> availableSkills;

	public SkillProgressionManager progressionManager = new SkillProgressionManager();

	private void Start() {
		PlayerSkillManager.Instance.Initialize();
		availableSkills.ForEach((eachAvailableSkill) => skillComponent.AddPlayerSkill(PlayerSkillManager.Instance.GetPlayerSkillData(eachAvailableSkill), 1, -1, -1, 0, 0, 0));
		progressionManager.CheckAndUnlock(currenciesComponent, PlayerSkillManager.Instance.GetPlayerSkillData(PLAYER_SKILL_TYPE.LIGHTNING));
	}
}
