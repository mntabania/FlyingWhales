﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UnlockMinionItemUI : MonoBehaviour {
    public static Action<PLAYER_SKILL_TYPE, int> onClickUnlockMinion;
    
    [SerializeField] private CharacterPortrait _portrait;
    [SerializeField] private TextMeshProUGUI lblName;
    [SerializeField] private TextMeshProUGUI lblCosts;
    [SerializeField] private GameObject goCheckMark;
    [SerializeField] private GameObject goPortraitCover;

    private PLAYER_SKILL_TYPE _minionType;
    private MinionPlayerSkill _skillData;
    public void SetMinionType(PLAYER_SKILL_TYPE p_type) {
        _minionType = p_type;
        string trimmed = p_type.ToString().Remove(0, 6);
        lblName.text = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetterOnly(trimmed);
        _skillData = PlayerSkillManager.Instance.GetMinionPlayerSkillData(p_type);
        lblCosts.text = $"{_skillData.unlockCost.ToString()}{UtilityScripts.Utilities.ManaIcon()}";
        _portrait.GeneratePortrait(CharacterManager.Instance.GeneratePortrait(RACE.DEMON, GENDER.MALE, _skillData.className, false));
        _portrait.AddPointerClickAction(OnClickMinionItem);
    }

    public void UpdateSelectableState() {
        if (PlayerManager.Instance.player.playerSkillComponent.minionsSkills.Contains(_minionType)) {
            //player already has minion type
            SetCheckmarkState(true);
            SetCoverState(true);
        } else {
            SetCheckmarkState(false);
            //player does not yet have minion type, check if player can afford to unlock this
            SetCoverState(PlayerManager.Instance.player.mana < _skillData.unlockCost);
        }
    }
    
    private void OnClickMinionItem() {
        onClickUnlockMinion?.Invoke(_minionType, _skillData.unlockCost);
    }

    public void SetCoverState(bool p_state) {
        goPortraitCover.SetActive(p_state);
    }
    public void SetCheckmarkState(bool p_state) {
        goCheckMark.SetActive(p_state);
    }
    
}
