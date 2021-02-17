using System.Collections;
using System;
using UnityEngine;
using Ruinarch.Custom_UI;
using UnityEngine.UI;

public class UpgradeSkillItemUI : MonoBehaviour {
	public Action<PLAYER_SKILL_TYPE> onButtonClick;
	public RuinarchButton btnSkill;
	public RuinarchText txtSkillName;
	public RuinarchText txtUpgrade;
	public RuinarchText txtCost;
	public Image spiritIcon;

	private PLAYER_SKILL_TYPE m_skillType;

	private void OnEnable() {
		btnSkill.onClick.AddListener(SkillClicked);
	}

	private void OnDisable() {
		btnSkill.onClick.RemoveListener(SkillClicked);
	}

	public void InitItem(PLAYER_SKILL_TYPE p_type, int p_spiritCount) {
		PlayerSkillData data = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(p_type);
		SkillData skillData = PlayerSkillManager.Instance.GetPlayerSkillData(p_type);
		if (skillData.currentLevel >= 3) {
			txtCost.text = "MAX";
			spiritIcon.gameObject.SetActive(false);
			btnSkill.gameObject.SetActive(false);
		} else {
			btnSkill.gameObject.SetActive(true);
			spiritIcon.gameObject.SetActive(true);
			txtCost.text = data.skillUpgradeData.GetUpgradeCostBaseOnLevel(skillData.currentLevel).ToString();
		}
		if (data.skillUpgradeData.bonuses.Count > 0) {
			txtUpgrade.text = data.skillUpgradeData.bonuses[0].ToString();
		} else {
			txtUpgrade.text = "n/a";
		}
		if (p_spiritCount < data.skillUpgradeData.GetUpgradeCostBaseOnLevel(skillData.currentLevel)) {
			btnSkill.interactable = false;
		} else {
			btnSkill.interactable = true;
		}
		
		txtSkillName.text = data.name;
		m_skillType = p_type;
	}

	private void SkillClicked() {
		onButtonClick?.Invoke(m_skillType);
	}
}
