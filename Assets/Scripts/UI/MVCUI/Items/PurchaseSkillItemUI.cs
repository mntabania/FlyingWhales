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

	public Sprite affliction;
	public Sprite spell;
	public Sprite playerAction;
	public Sprite minion;
	public Sprite passive;
	public Sprite structure;

	private PLAYER_SKILL_TYPE m_skillType;

	private void OnEnable() {
		btnSkill.onClick.AddListener(SkillClicked);
	}

	private void OnDisable() {
		btnSkill.onClick.RemoveListener(SkillClicked);
	}

	public void InitItem(PLAYER_SKILL_TYPE p_type) {
		PlayerSkillData data = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(p_type);
		SkillData skillData = PlayerSkillManager.Instance.GetPlayerSkillData(p_type);
		txtSkillName.text = data.name;
		txtDescription.text = skillData.description;
		switch (skillData.category) {
			case PLAYER_SKILL_CATEGORY.AFFLICTION:
			imgIcon.sprite = affliction;
			break;
			case PLAYER_SKILL_CATEGORY.SPELL:
			imgIcon.sprite = spell;
			break;
			case PLAYER_SKILL_CATEGORY.PLAYER_ACTION:
			imgIcon.sprite = playerAction;
			break;
			case PLAYER_SKILL_CATEGORY.MINION:
			case PLAYER_SKILL_CATEGORY.SUMMON:
			imgIcon.sprite = minion;
			break;
			case PLAYER_SKILL_CATEGORY.DEMONIC_STRUCTURE:
			imgIcon.sprite = structure;
			break;
			case PLAYER_SKILL_CATEGORY.SCHEME:
			imgIcon.sprite = passive;
			break;
		}
		txtLevel.text = "Level 0";
		txtCost.text = data.unlockCost.ToString();
		m_skillType = p_type;
	}

	private void SkillClicked() {
		onButtonClick?.Invoke(m_skillType);
	}
}
