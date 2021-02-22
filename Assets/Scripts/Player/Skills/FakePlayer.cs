using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakePlayer : MonoBehaviour
{
	public CurrenciesComponent currenciesComponent = new CurrenciesComponent();
    public PlayerSkillComponent skillComponent = new PlayerSkillComponent();

	public List<PLAYER_SKILL_TYPE> availableSkills;

	public SkillProgressionManager progressionManager = new SkillProgressionManager();

	public void Initialize() {
		PlayerSkillManager.Instance.Initialize();
		availableSkills.ForEach((eachAvailableSkill) => skillComponent.SetPlayerSkillData(eachAvailableSkill, true));
		progressionManager.CheckAndUnlock(skillComponent, currenciesComponent, PLAYER_SKILL_TYPE.LIGHTNING);
	}
}
