using System.Collections;
using System;
using UnityEngine;
using Ruinarch.Custom_UI;
using UnityEngine.UI;

public class PurchaseSkillItemUI : MonoBehaviour
{
    public Action<PLAYER_SKILL_TYPE> onButtonClick;
    public RuinarchButton btnSkill;
	public RuinarchText txtSkillName;
	public RuinarchText txtDescription;
	public RuinarchText txtLevel;
	public RuinarchText txtCost;
	public Image imgIcon;

	private PLAYER_SKILL_TYPE m_skillType;

	private void OnEnable() {
		btnSkill.onClick.AddListener(SkillClicked);
	}

	private void OnDisable() {
		btnSkill.onClick.RemoveListener(SkillClicked);
	}

	public void InitItem(PLAYER_SKILL_TYPE p_type) {
		PlayerSkillData data = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(p_type);
		txtSkillName.text = data.name;
		txtDescription.text = data.name;
		imgIcon.sprite = data.playerActionIcon;
		txtLevel.text = "Level 0";
		txtCost.text = data.unlockCost.ToString();
		m_skillType = p_type;
	}

	private void SkillClicked() {
		onButtonClick?.Invoke(m_skillType);
	}
}
