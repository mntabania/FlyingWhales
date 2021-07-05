using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakePlayer : MonoBehaviour
{
	public CurrenciesComponent currenciesComponent = new CurrenciesComponent();
    public PlayerSkillComponent skillComponent = new PlayerSkillComponent();

	public List<PLAYER_SKILL_TYPE> availableSkills;
	public List<SUMMON_TYPE> summons;

	public SkillProgressionManager progressionManager = new SkillProgressionManager();
	public PlayerUnderlingsComponent underlingsComponent { get; private set; }

	public void Initialize() {
		PlayerSkillManager.Instance.Initialize();
		availableSkills.ForEach((eachAvailableSkill) => skillComponent.AddAndCategorizePlayerSkill(eachAvailableSkill, true));
		progressionManager.CheckRequirementsAndGetUnlockCost(skillComponent, currenciesComponent, PLAYER_SKILL_TYPE.LIGHTNING);
		underlingsComponent = new PlayerUnderlingsComponent();
		summons.ForEach((eachSummon) => {
			underlingsComponent.AdjustMonsterUnderlingCharge(eachSummon, 5);
		});
	}
}
