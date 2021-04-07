﻿using System;
using EZObjectPools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePortalItemUI : PooledObject {

    [SerializeField] private BaseCharacterPortrait characterPortrait;
    [SerializeField] private BaseLocationPortrait locationPortrait;
    [SerializeField] private GameObject goPassiveSkillPortrait;
    [SerializeField] private TextMeshProUGUI lblName;
    [SerializeField] private RectTransform _contentParent;
    [SerializeField] private CanvasGroup _canvasGroupContent;
    private PLAYER_SKILL_TYPE _skill;
    private System.Action<UpgradePortalItemUI> _onHoverOverItem;
    private System.Action<UpgradePortalItemUI> _onHoverOutItem;

    #region getters
    public PLAYER_SKILL_TYPE skill => _skill;
    public RectTransform contentParent => _contentParent;
    public CanvasGroup canvasGroupContent => _canvasGroupContent;
    #endregion
    
    private void Awake() {
        characterPortrait.AddHoverOverAction(OnHoverOverItem);
        locationPortrait.AddHoverOverAction(OnHoverOverItem);
        
        characterPortrait.AddHoverOutAction(OnHoverOutItem);
        locationPortrait.AddHoverOutAction(OnHoverOutItem);
    }
    public void SetData(PLAYER_SKILL_TYPE p_type) {
        SkillData skillData = PlayerSkillManager.Instance.GetSkillData(p_type);
        _skill = p_type;
        if (skillData.category == PLAYER_SKILL_CATEGORY.MINION) {
            MinionPlayerSkill minionPlayerSkill = PlayerSkillManager.Instance.GetMinionPlayerSkillData(p_type);
            locationPortrait.gameObject.SetActive(false);
            characterPortrait.gameObject.SetActive(true);
            goPassiveSkillPortrait.SetActive(false);
            characterPortrait.GeneratePortrait(minionPlayerSkill.minionType);
            lblName.text = minionPlayerSkill.name;
        } else if (skillData.category == PLAYER_SKILL_CATEGORY.DEMONIC_STRUCTURE) {
            DemonicStructurePlayerSkill demonicStructurePlayerSkill = PlayerSkillManager.Instance.GetDemonicStructureSkillData(p_type);
            locationPortrait.gameObject.SetActive(true);
            characterPortrait.gameObject.SetActive(false);
            goPassiveSkillPortrait.SetActive(false);
            locationPortrait.SetPortrait(demonicStructurePlayerSkill.structureType);
            lblName.text = demonicStructurePlayerSkill.name;
        }
    }
    public void SetData(PASSIVE_SKILL p_passive) {
        PassiveSkill passiveSkill = PlayerSkillManager.Instance.GetPassiveSkill(p_passive);
        locationPortrait.gameObject.SetActive(false);
        characterPortrait.gameObject.SetActive(false);
        goPassiveSkillPortrait.SetActive(true);
        lblName.text = passiveSkill.name;
    }

    public void AddHoverOverAction(System.Action<UpgradePortalItemUI> p_action) {
        _onHoverOverItem += p_action;
    }
    public void AddHoverOutAction(System.Action<UpgradePortalItemUI> p_action) {
        _onHoverOutItem += p_action;
    }
    private void OnHoverOverItem() {
        _onHoverOverItem?.Invoke(this);
    }
    private void OnHoverOutItem() {
        _onHoverOutItem?.Invoke(this);
    }
}
