using System.Collections;
using System;
using Coffee.UIExtensions;
using DG.Tweening;
using UnityEngine;
using Ruinarch.Custom_UI;
using UnityEngine.UI;

public class PurchaseSkillItemUI : MonoBehaviour {
    public Action<PLAYER_SKILL_TYPE> onButtonClick;
    public Action<PlayerSkillData, PurchaseSkillItemUI> onHoverOver;
    public Action<PlayerSkillData, PurchaseSkillItemUI> onHoverOut;
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

	public Image disabler;

	public HoverHandler hoverHandler;
	public UIShiny borderShineEffect;

	public RectTransform rectTransformContent;
	public CanvasGroup canvasGroupContent;
	public CanvasGroup canvasGroupPortrait;
	public CanvasGroup canvasGroupSpellText;
	public CanvasGroup canvasGroupCurrencies;
	public UIShiny mainShineEffect;

	private PLAYER_SKILL_TYPE m_skillType;

	PlayerSkillData m_data;
	public RuinarchText bonusCharges;
	[SerializeField] private AnimationCurve _animationCurve;

	private Vector2 _defaultContentSize;
	private void Awake() {
		_defaultContentSize = rectTransformContent.sizeDelta;
	}
	private void OnEnable() {
		btnSkill.onClick.AddListener(SkillClicked);
		hoverHandler.AddOnHoverOverAction(OnHoverOver);
		hoverHandler.AddOnHoverOutAction(OnHoverOut);
	}

	private void OnDisable() {
		btnSkill.onClick.RemoveListener(SkillClicked);
		hoverHandler.RemoveOnHoverOverAction(OnHoverOver);
		hoverHandler.RemoveOnHoverOutAction(OnHoverOut);
	}

	public void InitItem(PLAYER_SKILL_TYPE p_type, int p_currentMana) {
		m_data = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(p_type);
		SkillData skillData = PlayerSkillManager.Instance.GetSkillData(p_type);
		string name = $"{UtilityScripts.Utilities.ColorizeSpellTitle($"{skillData.name}")} {("x" + $"{m_data.bonusChargeWhenUnlocked}")} {UtilityScripts.Utilities.BonusChargesIcon()}";
		txtSkillName.text = name;
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
		bonusCharges.text = "x" + m_data.bonusChargeWhenUnlocked.ToString();
		txtLevel.text = "Level 0";
		txtCost.text = m_data.GetUnlockCost().ToString();
		m_skillType = p_type;
		UpdateItem(p_currentMana);
		hoverHandler.ExecuteHoverEnterActionPerFrame(m_data.GetUnlockCost() > p_currentMana);
		transform.localScale = Vector3.one;
	}

	public void UpdateItem(int p_currentMana) {
		if (PlayerManager.Instance.player.playerSkillComponent.currentSpellBeingUnlocked != PLAYER_SKILL_TYPE.NONE) {
			if (PlayerManager.Instance.player.playerSkillComponent.currentSpellBeingUnlocked == m_skillType) {
				btnSkill.interactable = false;
				disabler.gameObject.SetActive(false);	
			} else {
				DisableButton();
			}
		} else if (m_data.GetUnlockCost() > p_currentMana) {
			DisableButton();
		} else {
			EnableButton();
		}
	}

	void DisableButton() {
		btnSkill.interactable = false;
		disabler.gameObject.SetActive(true);
	}

	void EnableButton() {
		btnSkill.interactable = true;
		disabler.gameObject.SetActive(false);
	}

	private void SkillClicked() {
		onButtonClick?.Invoke(m_skillType);
	}
	private void OnHoverOver() {
		onHoverOver?.Invoke(m_data, this);
	}
	private void OnHoverOut() {
		onHoverOut?.Invoke(m_data, this);
	}

	#region Animation
	public Sequence PrepareAnimation() {
		Sequence sequence = DOTween.Sequence();
		Vector2 targetSize = _defaultContentSize;
		rectTransformContent.sizeDelta = new Vector2(targetSize.x - 30f, targetSize.y - 30f);
		canvasGroupContent.alpha = 0f;
		canvasGroupPortrait.alpha = 0f;
		canvasGroupSpellText.alpha = 0f;
		canvasGroupCurrencies.alpha = 0f;
	        
		sequence.Append(rectTransformContent.DOSizeDelta(targetSize, 0.3f).SetEase(_animationCurve).OnPlay(() => mainShineEffect.Play()));
		sequence.Join(canvasGroupContent.DOFade(1f, 0.2f));
		sequence.Join(canvasGroupPortrait.DOFade(1f, 0.2f).SetDelay(0.2f));
		sequence.Join(canvasGroupSpellText.DOFade(1f, 0.2f).SetDelay(0.3f));
		sequence.Join(canvasGroupCurrencies.DOFade(1f, 0.2f).SetDelay(0.4f));
		sequence.OnKill(() => SetContentSize(_defaultContentSize));
		sequence.OnComplete(() => SetContentSize(_defaultContentSize));
		
		return sequence;
	}
	private void SetContentSize(Vector2 size) {
		rectTransformContent.sizeDelta = size;
	}
	#endregion
}
