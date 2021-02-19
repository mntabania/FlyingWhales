﻿using System.Collections;
using System;
using UnityEngine;
using Ruinarch.Custom_UI;
using UnityEngine.UI;
using TMPro;

public class SkillUpgradeItemUI : MonoBehaviour {
	public Action<PLAYER_SKILL_TYPE> onButtonClick;
	public RuinarchButton btnSkill;
	public RuinarchText txtSkillName;
	public RuinarchText txtUpgrade;
	public RuinarchText txtCost;
	public Image spiritIcon;
	public RuinarchText txtPlus;

	private PLAYER_SKILL_TYPE m_skillType;

	private void OnEnable() {
		btnSkill.onClick.AddListener(SkillClicked);
	}

	private void OnDisable() {
		btnSkill.onClick.RemoveListener(SkillClicked);
	}

	public void InitItem(PLAYER_SKILL_TYPE p_type, int p_spiritCount) {
		PlayerSkillData playerSkillData = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(p_type);
		SkillData skillData = PlayerSkillManager.Instance.GetPlayerSkillData(p_type);
		if (skillData.currentLevel >= 3) {
			txtCost.text = "MAX";
			spiritIcon.gameObject.SetActive(false);
			btnSkill.gameObject.SetActive(false);
		} else {
			btnSkill.gameObject.SetActive(true);
			spiritIcon.gameObject.SetActive(true);
			txtCost.text = playerSkillData.skillUpgradeData.GetUpgradeCostBaseOnLevel(skillData.currentLevel).ToString();

			if (p_spiritCount < playerSkillData.skillUpgradeData.GetUpgradeCostBaseOnLevel(skillData.currentLevel + 1)) {
				btnSkill.interactable = false;
				txtPlus.color = new Color32(128, 128, 128, 128);
			} else {
				btnSkill.interactable = true;
				txtPlus.color = new Color32(255, 255, 0,  255);
			}
		}
		if (playerSkillData.skillUpgradeData.bonuses.Count > 0) {
			txtUpgrade.text = playerSkillData.skillUpgradeData.GetDescriptionBaseOnFirstBonus(skillData.currentLevel + 1);
		} else {
			txtUpgrade.text = "n/a";
		}
		
		txtSkillName.text = playerSkillData.name;
		m_skillType = p_type;
	}

	private void SkillClicked() {
		onButtonClick?.Invoke(m_skillType);
	}
}
