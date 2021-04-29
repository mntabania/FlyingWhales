using System;
using EZObjectPools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePortalItemUI : PooledObject {

    [SerializeField] private BaseCharacterPortrait characterPortrait;
    [SerializeField] private BaseLocationPortrait locationPortrait;
    [SerializeField] private GameObject goPassiveSkillPortrait;
    [SerializeField] private GameObject goSpellPortrait;
    [SerializeField] private Image imgSpellPortrait;
    [SerializeField] private HoverHandler hoverHandlerSpellPortrait;
    [SerializeField] private TextMeshProUGUI lblName;
    [SerializeField] private RectTransform _contentParent;
    [SerializeField] private CanvasGroup _canvasGroupContent;
    
    [Header("Icons")]
    [SerializeField] private Sprite spriteAffliction;
    [SerializeField] private Sprite spriteSpell;
    [SerializeField] private Sprite spritePlayerAction;
    [SerializeField] private Sprite spriteMinion;
    [SerializeField] private Sprite spriteStructure;
    
    //NOTE: Only 1 of these values can have value, either skill or passive skill.
    private PLAYER_SKILL_TYPE _skill;
    private PASSIVE_SKILL _passiveSkill;
    private System.Action<UpgradePortalItemUI> _onHoverOverItem;
    private System.Action<UpgradePortalItemUI> _onHoverOutItem;
    private bool _allowHoverInteraction = true;
    
    #region getters
    public PLAYER_SKILL_TYPE skill => _skill;
    public PASSIVE_SKILL passiveSkill => _passiveSkill;
    public RectTransform contentParent => _contentParent;
    public CanvasGroup canvasGroupContent => _canvasGroupContent;
    #endregion
    
    private void Awake() {
        characterPortrait.AddHoverOverAction(OnHoverOverItem);
        locationPortrait.AddHoverOverAction(OnHoverOverItem);
        hoverHandlerSpellPortrait.AddOnHoverOverAction(OnHoverOverItem);
        
        characterPortrait.AddHoverOutAction(OnHoverOutItem);
        locationPortrait.AddHoverOutAction(OnHoverOutItem);
        hoverHandlerSpellPortrait.AddOnHoverOutAction(OnHoverOutItem);
    }
    public void SetData(PLAYER_SKILL_TYPE p_type) {
        SkillData skillData = PlayerSkillManager.Instance.GetSkillData(p_type);
        _skill = p_type;
        if (skillData.category == PLAYER_SKILL_CATEGORY.MINION) {
            MinionPlayerSkill minionPlayerSkill = PlayerSkillManager.Instance.GetMinionPlayerSkillData(p_type);
            locationPortrait.gameObject.SetActive(false);
            characterPortrait.gameObject.SetActive(true);
            goPassiveSkillPortrait.SetActive(false);
            goSpellPortrait.SetActive(false);
            characterPortrait.GeneratePortrait(minionPlayerSkill.minionType);
            lblName.text = minionPlayerSkill.name;
        } else if (skillData.category == PLAYER_SKILL_CATEGORY.DEMONIC_STRUCTURE) {
            DemonicStructurePlayerSkill demonicStructurePlayerSkill = PlayerSkillManager.Instance.GetDemonicStructureSkillData(p_type);
            locationPortrait.gameObject.SetActive(true);
            characterPortrait.gameObject.SetActive(false);
            goPassiveSkillPortrait.SetActive(false);
            goSpellPortrait.SetActive(false);
            locationPortrait.SetPortrait(demonicStructurePlayerSkill.structureType);
            lblName.text = demonicStructurePlayerSkill.name;
        } else {
            Sprite sprite = GetPowerSprite(skillData.category);
            locationPortrait.gameObject.SetActive(false);
            characterPortrait.gameObject.SetActive(false);
            goPassiveSkillPortrait.SetActive(false);
            goSpellPortrait.SetActive(true);
            imgSpellPortrait.sprite = sprite;
            lblName.text = skillData.name;
        }
    }
    private Sprite GetPowerSprite(PLAYER_SKILL_CATEGORY p_category) {
        switch (p_category) {
            case PLAYER_SKILL_CATEGORY.SPELL:
                return spriteSpell;
            case PLAYER_SKILL_CATEGORY.AFFLICTION:
                return spriteAffliction;
            case PLAYER_SKILL_CATEGORY.PLAYER_ACTION:
                return spritePlayerAction;
            case PLAYER_SKILL_CATEGORY.DEMONIC_STRUCTURE:
                return spriteStructure;
            case PLAYER_SKILL_CATEGORY.MINION:
                return spriteMinion;
            default:
                return null;
        }
    }
    // public void SetData(PASSIVE_SKILL p_passive) {
    //     PassiveSkill passiveSkill = PlayerSkillManager.Instance.GetPassiveSkill(p_passive);
    //     locationPortrait.gameObject.SetActive(false);
    //     characterPortrait.gameObject.SetActive(false);
    //     goSpellPortrait.SetActive(false);
    //     goPassiveSkillPortrait.SetActive(true);
    //     lblName.text = passiveSkill.name;
    // }

    public void AddHoverOverAction(System.Action<UpgradePortalItemUI> p_action) {
        _onHoverOverItem += p_action;
    }
    public void AddHoverOutAction(System.Action<UpgradePortalItemUI> p_action) {
        _onHoverOutItem += p_action;
    }
    private void OnHoverOverItem() {
        if (_allowHoverInteraction) {
            _onHoverOverItem?.Invoke(this);    
        }
    }
    private void OnHoverOutItem() {
        if (_allowHoverInteraction) {
            _onHoverOutItem?.Invoke(this);    
        }
    }
    public void SetHoverInteractionAllowedState(bool p_state) {
        _allowHoverInteraction = p_state;
    }
    public override void Reset() {
        base.Reset();
        _allowHoverInteraction = true;
        _skill = PLAYER_SKILL_TYPE.NONE;
        _passiveSkill = PASSIVE_SKILL.None;
    }
}
